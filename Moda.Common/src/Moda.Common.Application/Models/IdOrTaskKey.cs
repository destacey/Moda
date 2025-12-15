using System.Linq.Expressions;
using OneOf;

namespace Moda.Common.Application.Models;

/// <summary>
/// Represents either a Guid ID or a TaskKey (string like "APOLLO-T001").
/// </summary>
public class IdOrTaskKey : OneOfBase<Guid, string>
{
    public IdOrTaskKey(OneOf<Guid, string> value) : base(value) { }

    public IdOrTaskKey(string value) : base(Guid.TryParse(value, out var guid) ? guid : value) { }

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
    /// Gets the value as a string TaskKey if it is a string; otherwise, null.
    /// </summary>
    public string? AsTaskKey => Value switch
    {
        string key => key,
        _ => null
    };

    /// <summary>
    /// Gets a value indicating whether the value is a TaskKey (string).
    /// </summary>
    public bool IsTaskKey => Value is string;

    /// <summary>
    /// Implicitly converts a Guid to an <see cref="IdOrTaskKey"/>.
    /// </summary>
    /// <param name="value">The Guid value to convert.</param>
    public static implicit operator IdOrTaskKey(Guid value) => new(value);

    /// <summary>
    /// Implicitly converts a string to an <see cref="IdOrTaskKey"/>.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    public static implicit operator IdOrTaskKey(string value) => new(value);
}

public static class IdOrTaskKeyExtensions
{
    /// <summary>
    /// Creates an expression filter for ProjectTask based on Id or TaskKey.
    /// </summary>
    /// <param name="idOrTaskKey"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CreateTaskFilter<T>(this IdOrTaskKey idOrTaskKey,
        Expression<Func<T, Guid>> idSelector,
        Expression<Func<T, string>> taskKeySelector)
    {
        var parameter = Expression.Parameter(typeof(T), "t");

        var idExpression = Expression.Invoke(idSelector, parameter);
        var taskKeyExpression = Expression.Invoke(taskKeySelector, parameter);

        var body = idOrTaskKey.Match(
            id => Expression.Equal(idExpression, Expression.Constant(id)),
            taskKey => Expression.Equal(taskKeyExpression, Expression.Constant(taskKey))
        );

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
