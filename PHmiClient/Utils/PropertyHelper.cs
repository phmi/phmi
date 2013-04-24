using System;
using System.Linq.Expressions;

namespace PHmiClient.Utils
{
    public abstract class PropertyHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T, object>> getPropertyExpression)
        {
            var unaryExpression = getPropertyExpression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var propertyExpression = (MemberExpression)unaryExpression.Operand;
                return propertyExpression.Member.Name;
            }
            var memberExpression = getPropertyExpression.Body as MemberExpression;
            if (memberExpression != null)
            {
                return memberExpression.Member.Name;
            }
            throw new ArgumentException("getPropertyExpression must be UnaryExpression or MemberExpression");
        }

        public static string GetPropertyName<T>(T obj, Expression<Func<T, object>> getPropertyExpression)
        {
            return GetPropertyName(getPropertyExpression);
        }
    }
}
