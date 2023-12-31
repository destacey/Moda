using Moda.Organization.Domain.Enums;
using Moda.Organization.Domain.Models;
using Moda.Planning.Domain.Models;
using Moda.Tests.Shared.Data;

namespace Moda.Planning.Domain.Tests.Data;
public class PlanningTeamFaker : PrivateConstructorFaker<PlanningTeam>
{
    public PlanningTeamFaker(TeamType type)
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.Name, f => f.Random.String2(15));
        RuleFor(x => x.Code, f => new TeamCode(f.Random.String2(10).ToUpper()));
        RuleFor(x => x.Type, type);
        RuleFor(x => x.IsActive, true);
    }
}
