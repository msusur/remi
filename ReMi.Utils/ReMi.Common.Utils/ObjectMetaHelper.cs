using System;
using System.Linq.Expressions;

namespace ReMi.Common.Utils
{
    public static class ObjectMetaHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            var body = expression.Body as MemberExpression ??
                       ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (body == null)
                return string.Empty;

            return body.Member.Name;
        }

        public static string PropertyName<T>(this T obj, Expression<Func<T, object>> expression)
        {
            return GetPropertyName(expression);
        }
    }
}
