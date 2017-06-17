using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EfCore.NestedSets.Tests
{
    public class Node : INestedSet<Node, int, int?>
    {
        public int Id { get; set; }
        public Node Parent { get; set; }
        public List<Node> Children { get; set; }
        public List<Node> Descendants { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public string Name { get; set; }
        public bool Moving { get; set; }
        public Node Root { get; set; }
        public int? RootId { get; set; }

        public Node() { }

        public Node(string name, int? parentId, int level, int left, int right)
            : this(0, parentId, level, left, right, name)
        {
        }

        public Node(int id, int? parentId, int level, int left, int right, string name)
        {
            Id = id;
            ParentId = parentId;
            Level = level;
            Left = left;
            Right = right;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}