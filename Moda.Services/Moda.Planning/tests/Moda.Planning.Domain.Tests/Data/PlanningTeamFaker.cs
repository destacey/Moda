using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Models.Organizations;
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
        RuleFor(x => x.Code, f => new TeamCode(f.Random.AlphaNumeric(5)));
        RuleFor(x => x.Type, type);
        RuleFor(x => x.IsActive, true);
    }
}
