using System.Linq.Expressions;
using System.Reflection;

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

    /// <summary>
    /// A generic method to get the value of a private or protected list field and add an item to it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="instance"></param>
    /// <param name="listFieldName"></param>
    /// <param name="item"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void AddToPrivateList<T, TItem>(this T instance, string listFieldName, TItem item)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if(string.IsNullOrWhiteSpace(listFieldName)) throw new ArgumentNullException(nameof(listFieldName));
        ArgumentNullException.ThrowIfNull(item);

        var fieldInfo = typeof(T).GetField(listFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        var value = fieldInfo?.GetValue(instance) 
            ?? throw new InvalidOperationException($"Could not get the {listFieldName} list from the {instance.GetType().Name} instance.");

        var list = (List<TItem>)value;

        list.Add(item);
    }

    /// <summary>
    /// A generic method to get a reference of a private or protected list field.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="listFieldName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static List<T> GetPrivateList<T>(object instance, string listFieldName)
    {
        var fieldInfo = instance.GetType().GetField(listFieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Could not find the {listFieldName} field on the {instance.GetType().Name} class.");

        return fieldInfo?.GetValue(instance) as List<T>
            ?? throw new InvalidOperationException($"Could not get the {listFieldName} list from the {instance.GetType().Name} instance.");
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
