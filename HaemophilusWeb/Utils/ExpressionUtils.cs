using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace HaemophilusWeb.Utils
{
    public static class ExpressionUtils
    {
        public static string GetDisplayName<TType, TMember>(this Expression<Func<TType, TMember>> expression)
        {
            var memberExpression = GetMemberExpression(expression);
            return memberExpression.Member.GetDisplayName();
        }

        public static string GetDisplayName(this MemberInfo member)
        {
            var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute == null ? member.Name : displayAttribute.Name;
        }

        public static Type GetMemberType<TType, TMember>(this Expression<Func<TType, TMember>> expression)
        {
            try
            {
                var member = GetMemberExpression((Expression) expression);
                return member.Type;
            }
            catch
            {
            }
            return typeof (object);
        }

        private static MemberExpression GetMemberExpression<TType, TMember>(Expression<Func<TType, TMember>> expression)
        {
            return GetMemberExpression((Expression) expression);
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                return GetMemberExpression(((UnaryExpression) expression).Operand);
            }
            if (expression is LambdaExpression)
            {
                var lambda = expression as LambdaExpression;
                var body = lambda.Body;
                return GetMemberExpression(body);
            }
            if (expression is MethodCallExpression)
            {
                var call = expression as MethodCallExpression;
                var calledObject = call.Object;
                if ((calledObject == null || calledObject.NodeType == ExpressionType.Constant) &&
                    call.Arguments.Count >= 1)
                {
                    return GetMemberExpression(call.Arguments[0]);
                }
                return GetMemberExpression(calledObject);
            }
            var memberExpression = expression as MemberExpression;
            if (memberExpression == null)
            {
                throw new InvalidOperationException("Expression must be a member expression");
            }
            return memberExpression;
        }

        public static bool HasAttribute<TType, TMember, TAttribute>(this Expression<Func<TType, TMember>> expression)
            where TAttribute : Attribute
        {
            return GetCustomAttribute<TType, TMember, TAttribute>(expression) != null;
        }

        public static TAttribute GetCustomAttribute<TType, TMember, TAttribute>(
            this Expression<Func<TType, TMember>> expression) where TAttribute : Attribute
        {
            var memberExpression = GetMemberExpression(expression);
            return memberExpression.Member.GetCustomAttribute<TAttribute>();
        }
    }
}