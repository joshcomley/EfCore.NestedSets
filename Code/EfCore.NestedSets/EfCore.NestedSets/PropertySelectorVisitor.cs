using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EfCore.NestedSets
{
    internal class PropertySelectorVisitor : ExpressionVisitor
    {
        private readonly List<PropertyInfo> _properties = new List<PropertyInfo>();

        internal PropertySelectorVisitor(Expression exp)
        {
            Visit(exp);
        }

        public PropertyInfo Property
        {
            get
            {
                return _properties.SingleOrDefault();
            }
        }

        public ICollection<PropertyInfo> Properties
        {
            get
            {
                return _properties;
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            PropertyInfo pinfo = node.Member as PropertyInfo;

            if (pinfo == null)
            {
                throw new InvalidOperationException("Member expression must be properties");
                //throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeProperties, node.Member.DeclaringType.FullName, node.Member.Name);
            }

            if (node.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new InvalidOperationException("Member expression must be bound to lambda parameter");
                //throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeBoundToLambdaParameter);
            }

            _properties.Add(pinfo);
            return node;
        }

        public static PropertyInfo GetSelectedProperty(Expression exp)
        {
            return new PropertySelectorVisitor(exp).Property;
        }

        public static ICollection<PropertyInfo> GetSelectedProperties(Expression exp)
        {
            return new PropertySelectorVisitor(exp).Properties;
        }

        public sealed override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return exp;
            }

            switch (exp.NodeType)
            {
                case ExpressionType.New:
                case ExpressionType.MemberAccess:
                case ExpressionType.Lambda:
                    return base.Visit(exp);
                default:
                    throw new NotSupportedException();
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> lambda)
        {
            if (lambda == null)
            {
                throw new ArgumentNullException(nameof(lambda));
            }

            if (lambda.Parameters.Count != 1)
            {
                throw new InvalidOperationException("Lambda Expression must have exactly one parameter");
            }

            Expression body = Visit(lambda.Body);

            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }
    }
}