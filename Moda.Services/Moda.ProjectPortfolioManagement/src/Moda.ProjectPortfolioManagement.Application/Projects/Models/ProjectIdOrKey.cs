using System.Linq.Expressions;
using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Domain.Interfaces;
using OneOf;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Models;

/// <summary>
/// Represents either a Guid ID or a string project key.
/// </summary>
public sealed class ProjectIdOrKey : OneOfBase<Guid, ProjectKey>
{
    public ProjectIdOrKey(OneOf<Guid, ProjectKey> value) : base(value) { }

    public ProjectIdOrKey(string value) : base(Guid.TryParse(value, out var guid) ? guid : new ProjectKey(value)) { }

    public static implicit operator ProjectIdOrKey(Guid value) => new(OneOf<Guid, ProjectKey>.FromT0(value));
    public static implicit operator ProjectIdOrKey(ProjectKey value) => new(OneOf<Guid, ProjectKey>.FromT1(value));
    public static implicit operator ProjectIdOrKey(string value) => new(value);
}

public static class ProjectIdOrKeyExtensions
{
    /// <summary>
    /// Creates a filter expression that matches entities by project ID or key.
    /// </summary>
    public static Expression<Func<T, bool>> CreateFilter<T>(this ProjectIdOrKey idOrKey)
        where T : IHasProjectIdAndKey
    {
        return idOrKey.Match<Expression<Func<T, bool>>>(
            id => x => x.Id == id,
            key => x => x.Key == key
        );
    }

    /// <summary>
    /// Creates a filter expression that matches entities by project ID or key.
    /// </summary>
    public static Expression<Func<T, bool>> CreateProjectFilter<T>(this ProjectIdOrKey idOrKey)
        where T : IHasProject
    {
        return idOrKey.Match<Expression<Func<T, bool>>>(
            id => x => x.ProjectId == id,
            key => x => x.Project.Key == key.Value
        );
    }
}
