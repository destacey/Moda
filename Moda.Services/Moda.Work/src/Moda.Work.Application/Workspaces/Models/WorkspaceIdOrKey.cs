using System.Linq.Expressions;
using Moda.Common.Models;
using Moda.Work.Domain.Interfaces;
using OneOf;

namespace Moda.Work.Application.Workspaces.Models;
public class WorkspaceIdOrKey : OneOfBase<Guid, WorkspaceKey>
{
    public WorkspaceIdOrKey(OneOf<Guid, WorkspaceKey> value) : base(value) { }
    public WorkspaceIdOrKey(string value) : base(Guid.TryParse(value, out var guid) ? guid : new WorkspaceKey(value)) { }

    public static implicit operator WorkspaceIdOrKey(Guid value) => new(OneOf<Guid, WorkspaceKey>.FromT0(value));
    public static implicit operator WorkspaceIdOrKey(WorkspaceKey value) => new(OneOf<Guid, WorkspaceKey>.FromT1(value));
    public static implicit operator WorkspaceIdOrKey(string value) => new(value);
}

public static class WorkspaceIdOrKeyExtensions
{
    /// <summary>
    /// Creates an expression based on the values from the WorkspaceIdOrKey for objects that implement HasWorkspaceIdAndKey.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idOrKey"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CreateFilter<T>(this WorkspaceIdOrKey idOrKey)
        where T : HasWorkspaceIdAndKey
    {
        return idOrKey.Match<Expression<Func<T, bool>>>(
            id => x => x.Id == id,
            key => x => x.Key == key
        );
    }

    /// <summary>
    /// Creates an expression based on the values from the WorkspaceIdOrKey for objects that implement HasWorkspace.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idOrKey"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CreateWorkspaceFilter<T>(this WorkspaceIdOrKey idOrKey)
        where T : HasWorkspace
    {
        // Match returns the predicate directly - no additional expression building needed
        return idOrKey.Match<Expression<Func<T, bool>>>(
            id => x => x.WorkspaceId == id,
            key => x => x.Workspace.Key == key
        );
    }

    /// <summary>
    /// Creates an expression based on the values from the WorkspaceIdOrKey and custom property selectors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idOrKey"></param>
    /// <param name="idSelector"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CreateFilter<T>(
        this WorkspaceIdOrKey idOrKey,
        Expression<Func<T, Guid>> idSelector,
        Expression<Func<T, WorkspaceKey>> keySelector)
    {
        return idOrKey.Match(
            id => idSelector.CreateEqualsPredicate(id),
            key => keySelector.CreateEqualsPredicate(key)
        );
    }

    // Helper extension to create equality predicates
    private static Expression<Func<T, bool>> CreateEqualsPredicate<T, TProperty>(
        this Expression<Func<T, TProperty>> selector,
        TProperty value)
    {
        var valueExpression = Expression.Constant(value, typeof(TProperty));
        var equalityExpression = Expression.Equal(selector.Body, valueExpression);
        return Expression.Lambda<Func<T, bool>>(equalityExpression, selector.Parameters);
    }
}
