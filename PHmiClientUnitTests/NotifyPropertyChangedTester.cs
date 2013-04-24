using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using System;
using System.ComponentModel;

namespace PHmiClientUnitTests
{
    public class NotifyPropertyChangedTester
    {
        public static void Test<TNotifyPropertyChanged, TValue>(TNotifyPropertyChanged target, Expression<Func<TNotifyPropertyChanged, TValue>> getPropertyExpression, TValue value)
            where TNotifyPropertyChanged : INotifyPropertyChanged
        {
            var property = GetProperty(getPropertyExpression);
            var count = 0;
            target.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == property.Name)
                        count++;
                };
            
            property.SetValue(target, value, new object[0]);
            const int expectedCount = 1;

            var errorMsg = string.Format("INotifyPropertyChanged not functioning for property \"{0}\"", property.Name);

            Assert.AreEqual(expectedCount, count, errorMsg);
        }

        private static PropertyInfo GetProperty<TNotifyPropertyChanged, TValue>(Expression<Func<TNotifyPropertyChanged, TValue>> getPropertyExpression)
        {
            var unaryExpression = getPropertyExpression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var propertyExpression = (MemberExpression)unaryExpression.Operand;
                return (PropertyInfo) propertyExpression.Member;
            }
            var memberExpression = getPropertyExpression.Body as MemberExpression;
            if (memberExpression != null)
            {
                return (PropertyInfo) memberExpression.Member;
            }
            throw new ArgumentException("invalid getPropertyExpression");
        }
    }
}
