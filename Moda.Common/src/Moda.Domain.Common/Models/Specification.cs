using System.Linq.Expressions;

namespace Moda.Common.Domain.Models;

public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>
    /// Indicates whether the specification is satisfied by a given object.
    /// </summary>
    /// <param name="obj">The object to which the specification is applied</param>
    /// <returns></returns>
    public bool IsSatisfiedBy(T obj)
    {
        return ToExpression().Compile()(obj);
    }

    /// <summary>
    /// Gets the Linq expression for the current specification.
    /// </summary>
    /// <returns></returns>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Implicity converts a specification to an expression.
    /// </summary>
    /// <param name="specification"></param>
    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
    {
        return specification.ToExpression();
    }
}
