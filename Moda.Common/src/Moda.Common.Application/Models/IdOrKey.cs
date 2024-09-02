using System.Linq.Expressions;
using OneOf;

namespace Moda.Common.Application.Models;

public class IdOrKey : OneOfBase<Guid, int>
{
    public IdOrKey(OneOf<Guid, int> value) : base(value) { }

    public IdOrKey(string value) : base(Guid.TryParse(value, out var guid) ? guid : int.Parse(value)) { }

    public static implicit operator IdOrKey(Guid value) => new(value);
    public static implicit operator IdOrKey(int value) => new(value);
    public static implicit operator IdOrKey(string value) => new(value);

}

public static class IdOrKeyExtensions
{
    /// <summary>
    /// Creates an expression based on the values from the IdOrKey and selectors.  This should only be used to create a single expression.  If you are looping through IdOrKey values then do it locally be faster (2-3x) and less memory allocation (35-45% of current usage).
    /// 
    /// Manual example:
    ///   var expression = idOrKey.Match(
    ///     id => (Expression<Func<T, bool>>)(x => x.Id == id),
    ///     key => (Expression<Func<T, bool>>)(x => x.Key == key)
    ///   );
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
