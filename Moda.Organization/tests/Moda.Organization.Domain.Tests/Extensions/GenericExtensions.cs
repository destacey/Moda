using System.Linq.Expressions;

namespace Moda.Organization.Domain.Tests.Extensions;
public static class GenericExtensions
{
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
