using System.Linq.Expressions;
using Moda.Common.Application.Models.Organizations;
using Moda.Work.Domain.Interfaces;

namespace Moda.Work.Application.Extensions;

public static class TeamIdOrCodeExtensions
{
    public static Expression<Func<T, bool>> CreateWorkTeamFilter<T>(this TeamIdOrCode idOrCode)
        where T : IHasOptionalWorkTeam
    {
        // Match returns the predicate directly - no additional expression building needed
        return idOrCode.Match<Expression<Func<T, bool>>>(
            id => x => x.TeamId == id,
            code => x => x.Team != null && x.Team.Code == code
        );
    }
}
