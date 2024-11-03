using System.Linq.Expressions;

namespace Moda.Tests.Shared.Extensions;
public static class GenericExtensions
{
    /// <summary>
    /// A generic method to get the value of a private or protected property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="instance"></param>
    /// <param name="propertyExpression"></param>
    /// <param name="value"></param>
    public static void SetPrivate<T, TValue>(this T instance, Expression<Func<T, TValue>> propertyExpression, TValue value)
    {
        instance?.GetType().GetProperty(GetName(propertyExpression))?.SetValue(instance, value, null);
    }

    private static string GetName<T, TValue>(Expression<Func<T, TValue>> exp)
    {
        MemberExpression? body = exp.Body as MemberExpression;

        if (body is null)
        {
            UnaryExpression ubody = (UnaryExpression)exp.Body;
            body = ubody.Operand as MemberExpression;
        }

        return body!.Member.Name;
    }
}
