using System.Linq.Expressions;
using Moda.Common.Domain.Interfaces.Organization;
using Moda.Common.Domain.Models.Organizations;
using OneOf;

namespace Moda.Common.Application.Models.Organizations;

public sealed class TeamIdOrCode : OneOfBase<Guid, TeamCode>
{
    public TeamIdOrCode(OneOf<Guid, TeamCode> value) : base(value) { }
    public TeamIdOrCode(string value) : base(Guid.TryParse(value, out var guid) ? guid : new TeamCode(value)) { }

    public static implicit operator TeamIdOrCode(Guid value) => new(OneOf<Guid, TeamCode>.FromT0(value));
    public static implicit operator TeamIdOrCode(TeamCode value) => new(OneOf<Guid, TeamCode>.FromT1(value));
    public static implicit operator TeamIdOrCode(string value) => new(value);
}

public static class TeamIdOrCodeExtensions
{
    /// <summary>
    /// Creates an expression based on the values from the TeamIdOrCode for objects that implement IHasTeamIdAndCode.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idOrCode"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CreateFilter<T>(this TeamIdOrCode idOrCode)
        where T : IHasTeamIdAndCode
    {
        return idOrCode.Match<Expression<Func<T, bool>>>(
            id => x => x.Id == id,
            code => x => x.Code == code
        );
    }
}
