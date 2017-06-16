using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace EfCore.NestedSets
{
    public class NestedSetManager<T, TKey, TNullableKey>
        where T : class, INestedSet<TKey, TNullableKey>
    {
        private readonly DbContext _db;
        private readonly DbSet<T> _nodes;

        public NestedSetManager(DbContext dbContext, DbSet<T> nodesSource)
        {
            _db = dbContext;
            _nodes = nodesSource;
        }

        private Expression<Func<T, bool>> KeyEqualsExpression(TKey key)
        {
            // TODO: Blog about needing a parameter name here, looks like a bug
            var parameterExpression = Expression.Parameter(typeof(T), "entity");
            var propertyName = nameof(INestedSet<string, string>.Id);
            if (string.IsNullOrEmpty(propertyName))
                throw new NotSupportedException();
            return Expression.Lambda<Func<T, bool>>(
                Expression.Equal(Expression.Property(parameterExpression, propertyName), Expression.Constant(key)),
                parameterExpression);
        }

        public List<T> Delete(TKey nodeId, bool soft = false)
        {
            var nodeToDelete = _nodes.Single(KeyEqualsExpression(nodeId));
            var nodeToDeleteLeft = nodeToDelete.Left;
            var difference = nodeToDelete.Right - nodeToDelete.Left + 1;
            var deleted = _nodes.Where(s => s.Left >= nodeToDelete.Left && s.Right <= nodeToDelete.Right).ToList();
            if (soft)
                foreach (var node in deleted)
                    node.Moving = true;
            else
                foreach (var node in deleted)
                    _nodes.Remove(node);
            var nodesToUpdate = _nodes.Where(s => s.Left > nodeToDelete.Left || s.Right >= nodeToDelete.Left).ToList();
            foreach (var nodeToUpdate in nodesToUpdate)
            {
                if (nodeToUpdate.Moving)
                    continue;
                if (nodeToUpdate.Left >= nodeToDeleteLeft)
                    nodeToUpdate.Left -= difference;
                nodeToUpdate.Right -= difference;
            }
            var minLeft = deleted.Min(s => s.Left) - 1;
            // Reset to 1
            foreach (var deletedNode in deleted)
            {
                deletedNode.Left -= minLeft;
                deletedNode.Right -= minLeft;
                deletedNode.ParentId = default(TNullableKey);
            }
            if (!soft)
            {
                _db.SaveChanges();
                foreach (var deletedSite in deleted)
                    deletedSite.Id = default(TKey);
            }
            return deleted;
        }

        public void MoveToParent(TKey nodeId, TNullableKey parentId,
            NestedSetInsertMode insertMode)
        {
            Move(nodeId, parentId, default(TNullableKey), insertMode);
        }

        public void MoveToSibling(TKey nodeId, TNullableKey siblingId,
            NestedSetInsertMode insertMode)
        {
            Move(nodeId, default(TNullableKey), siblingId, insertMode);
        }

        private void Move(TKey nodeId, TNullableKey toParentId, TNullableKey toSiblingId,
            NestedSetInsertMode insertMode)
        {
            //var node = _nodes.Single(KeyEqualsExpression(nodeId));
            var deletedNodes = Delete(nodeId);
            Insert(toParentId, toSiblingId, deletedNodes, insertMode);
        }

        public T InsertRoot(T node,
            NestedSetInsertMode insertMode)
        {
            return Insert(default(TNullableKey), default(TNullableKey), new []{node}, insertMode).First();
        }

        public List<T> InsertRoot(IEnumerable<T> nodeTree,
            NestedSetInsertMode insertMode)
        {
            return Insert(default(TNullableKey), default(TNullableKey), nodeTree, insertMode);
        }

        public T InsertBelow(TNullableKey parentId, T node,
            NestedSetInsertMode insertMode)
        {
            return Insert(parentId, default(TNullableKey), new[] { node }, insertMode).First();
        }

        public List<T> InsertBelow(TNullableKey parentId, IEnumerable<T> nodeTree,
            NestedSetInsertMode insertMode)
        {
            return Insert(parentId, default(TNullableKey), nodeTree, insertMode);
        }

        public T InsertNextTo(TNullableKey siblingId, T node,
            NestedSetInsertMode insertMode)
        {
            return Insert(default(TNullableKey), siblingId, new[] { node }, insertMode).First();
        }

        public T InsertNextTo(TNullableKey siblingId, List<T> nodeTree,
            NestedSetInsertMode insertMode)
        {
            return Insert(default(TNullableKey), siblingId, nodeTree, insertMode).First();
        }

        private List<T> Insert(TNullableKey parentId, TNullableKey siblingId, IEnumerable<T> nodeTree,
            NestedSetInsertMode insertMode)
        {
            var nodeArray = nodeTree as T[] ?? nodeTree.ToArray();
            var lowestLeft = nodeArray.Min(n => n.Left);
            var highestRight = nodeArray.Max(n => n.Right);
            if (lowestLeft == 0 && highestRight == 0)
                if (nodeArray.Length == 1)
                {
                    var node = nodeArray.Single();
                    node.Left = 1;
                    node.Right = 2;
                    lowestLeft = 1;
                    highestRight = 2;
                }
                else
                {
                    throw new ArgumentException("Node tree must have left right values", nameof(nodeTree));
                }
            var difference = highestRight - lowestLeft;
            var nodeTreeRoot = nodeArray.Single(n => n.Left == lowestLeft);
            T parent = null;
            T sibling = null;
            if (!Equals(parentId, default(TNullableKey)) &&
                insertMode == NestedSetInsertMode.Right)
            {
                parent = _nodes.Single(KeyEqualsExpression((TKey)(object)parentId));
                if (parent == null)
                {
                    throw new ArgumentException(string.Format("Unable to find node parent with ID of {0}", parentId));
                }
                var parent1 = parent;
                var rightMostImmediateChild = _nodes
                    .Where(s => s.Left >= parent1.Left && s.Right <= parent1.Right && s.Level == parent1.Level + 1)
                    .OrderByDescending(s => s.Right)
                    .ToList()
                    .FirstOrDefault(n => !n.Moving)
                    ;
                sibling = rightMostImmediateChild;
                if (sibling != null)
                    siblingId = (TNullableKey)(object)sibling.Id;
            }
            int? siblingLeft = null;
            int? siblingRight = null;
            if (!Equals(siblingId, default(TNullableKey)))
            {
                if (sibling == null)
                    sibling = _nodes.Single(KeyEqualsExpression((TKey)(object)siblingId));
                siblingLeft = sibling.Left;
                siblingRight = sibling.Right;
                parentId = sibling.ParentId;
            }
            int? parentLeft = null;
            if (!Equals(parentId, null))
            {
                if (parent == null)
                    parent = _nodes.Single(KeyEqualsExpression((TKey)(object)parentId));
                parentLeft = parent.Left;
            }
            var minLevel = nodeArray.Min(n => n.Level);
            foreach (var node in nodeArray)
            {
                node.Level -= minLevel;
                if (parent != null)
                    node.Level += parent.Level + 1;
            }
            var left = 0;
            var right = 0;
            switch (insertMode)
            {
                case NestedSetInsertMode.Left:
                    {
                        IEnumerable<T> nodes;
                        if (sibling != null)
                        {
                            nodes = _nodes.Where(s => s.Left >= siblingLeft || s.Right >= siblingRight).ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = sibling.Left;
                            right = sibling.Left + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left >= siblingLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else if (parent != null)
                        {
                            nodes = _nodes.Where(s => s.Right >= parentLeft).ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = parent.Left + 1;
                            right = left + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left > parentLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else
                        {
                            left = 1;
                            right = 1 + difference;
                        }
                    }
                    break;
                case NestedSetInsertMode.Right:
                    {
                        List<T> nodes;
                        if (sibling != null)
                        {
                            nodes = _nodes.Where(s => s.Left > siblingRight || s.Right > siblingRight)
                                .ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = sibling.Right + 1;
                            right = sibling.Right + 1 + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left > siblingLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else if (parent != null)
                        {
                            nodes = _nodes.Where(s => s.Right >= parentLeft).ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = parent.Left + 1;
                            right = left + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left > parentLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else
                        {
                            left = 1;
                            right = 1 + difference;
                        }
                    }
                    break;
            }
            var leftChange = left - nodeTreeRoot.Left;
            var rightChange = right - nodeTreeRoot.Right;
            foreach (var node in nodeArray)
            {
                node.Left += leftChange;
                node.Right += rightChange;
            }
            nodeTreeRoot.ParentId = parentId;
            var newNodes = nodeArray.Where(n => !n.Moving).ToList();
            if (newNodes.Any())
            {
                _nodes.AddRange(newNodes);
            }
            var movingNodes = nodeArray.Where(n => n.Moving).ToList();
            foreach (var node in movingNodes)
                node.Moving = false;
            _db.SaveChanges();
            // Update the parent IDs now we have them
            foreach (var newNode in newNodes)
            {
                if (newNode != nodeTreeRoot)
                {
                    var path = newNodes.Where(n => n.Left < newNode.Left && n.Right > newNode.Right).OrderByDescending(n => n.Left)
                        .ToList();
                    var current = newNode;
                    foreach (var ancestor in path)
                    {
                        current.ParentId = (TNullableKey)(object)ancestor.Id;
                        current = ancestor;
                    }
                }
            }
            _db.SaveChanges();
            return newNodes;
        }
    }
}