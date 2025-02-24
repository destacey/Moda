using System.Linq.Expressions;
using Moda.Common.Domain.Interfaces;
using OneOf;

namespace Moda.Common.Application.Models;

public class IdOrKey : OneOfBase<Guid, int>
{
    public IdOrKey(OneOf<Guid, int> value) : base(value) { }

    public IdOrKey(string value) : base(Guid.TryParse(value, out var guid) ? guid : int.Parse(value)) { }

    /// <summary>
    /// Gets the value as a Guid if it is a Guid; otherwise, null.
    /// </summary>
    public Guid? AsId => Value switch
    {
        Guid id => id,
        _ => null
    };

    /// <summary>
    /// Gets a value indicating whether the value is a Guid.
    /// </summary>
    public bool IsId => Value is Guid;

    /// <summary>
    /// Gets the value as an int if it is an int; otherwise, null.
    /// </summary>
    public int? AsKey => Value switch
    {
        int key => key,
        _ => null
    };

    /// <summary>
    /// Gets a value indicating whether the value is an int.
    /// </summary>
    public bool IsKey => Value is int;

    /// <summary>
    /// Implicitly converts a Guid to an <see cref="IdOrKey"/>.
    /// </summary>
    /// <param name="value">The Guid value to convert.</param>
    public static implicit operator IdOrKey(Guid value) => new(value);

    /// <summary>
    /// Implicitly converts an int to an <see cref="IdOrKey"/>.
    /// </summary>
    /// <param name="value">The int value to convert.</param>
    public static implicit operator IdOrKey(int value) => new(value);

    /// <summary>
    /// Implicitly converts a string to an <see cref="IdOrKey"/>.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    public static implicit operator IdOrKey(string value) => new(value);
}

public static class IdOrKeyExtensions
{
    /// <summary>
    /// Creates an expression based on the values from the IdOrKey for objects that implement HasIdAndKey.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idOrKey"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CreateFilter<T>(this IdOrKey idOrKey) where T : HasIdAndKey
    {
        return idOrKey.Match<Expression<Func<T,bool>>>(
            id => (x => x.Id == id),
            key => (x => x.Key == key)
        );
    }

    /// <summary>
    /// Creates an expression based on the values from the IdOrKey and selectors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idOrKey"></param>
    /// <param name="idSelector"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CreateFilter<T>(this IdOrKey idOrKey, Expression<Func<T, Guid>> idSelector, Expression<Func<T, int>> keySelector)
    {
        var parameter = Expression.Parameter(typeof(T), "f");

        var idExpression = Expression.Invoke(idSelector, parameter);
        var keyExpression = Expression.Invoke(keySelector, parameter);

        var body = idOrKey.Match(
            id => Expression.Equal(idExpression, Expression.Constant(id)),
            key => Expression.Equal(keyExpression, Expression.Constant(key))
        );

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
