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
    /// Sets the value of a private or protected field on an instance.
    /// Useful for configuring Bogus fakers that need to set private fields.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <typeparam name="TValue">The type of the field value</typeparam>
    /// <param name="instance">The instance to modify</param>
    /// <param name="fieldName">The name of the private field (e.g., "_workStatus")</param>
    /// <param name="value">The value to set</param>
    public static void SetPrivateField<T, TValue>(this T instance, string fieldName, TValue value)
    {
        var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(instance, value);
    }

    /// <summary>
    /// A generic method to get the value of a private or protected list field and add an item to it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="instance"></param>
    /// <param name="fieldName"></param>
    /// <param name="item"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void AddToPrivateList<T, TItem>(this T instance, string fieldName, TItem item)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if(string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));
        ArgumentNullException.ThrowIfNull(item);

        var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        var value = fieldInfo?.GetValue(instance) 
            ?? throw new InvalidOperationException($"Could not get the {fieldName} list from the {instance.GetType().Name} instance.");

        var list = (List<TItem>)value;

        list.Add(item);
    }

    /// <summary>
    /// A generic method to get a reference of a private or protected list field.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static List<T> GetPrivateList<T>(object instance, string fieldName)
    {
        var fieldInfo = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Could not find the {fieldName} field on the {instance.GetType().Name} class.");

        return fieldInfo?.GetValue(instance) as List<T>
            ?? throw new InvalidOperationException($"Could not get the {fieldName} list from the {instance.GetType().Name} instance.");
    }

    /// <summary>
    /// A generic method to get a reference of a private or protected hashset field.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static HashSet<T> GetPrivateHashSet<T>(object instance, string fieldName)
    {
        var fieldInfo = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Could not find the {fieldName} field on the {instance.GetType().Name} class.");

        return fieldInfo?.GetValue(instance) as HashSet<T>
            ?? throw new InvalidOperationException($"Could not get the {fieldName} list from the {instance.GetType().Name} instance.");
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
