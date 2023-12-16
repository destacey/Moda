using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models;

namespace Moda.Planning.Domain.Tests.Data;
public class PlanningIntervalObjectiveFaker : Faker<PlanningIntervalObjective>
{
    public PlanningIntervalObjectiveFaker(Guid planningIntervalId, PlanningTeam team, ObjectiveStatus status, bool isStretch)
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.PlanningIntervalId, planningIntervalId);
        RuleFor(x => x.TeamId, team.Id);
        RuleFor(x => x.Team, team);
        RuleFor(x => x.ObjectiveId, f => f.Random.Guid());        
        RuleFor(x => x.Type, SetType(team));
        RuleFor(x => x.Status, status);
        RuleFor(x => x.IsStretch, isStretch);
    }

    private static PlanningIntervalObjectiveType SetType(PlanningTeam team)
    {
        return team.Type switch
        {
            Organization.Domain.Enums.TeamType.Team => PlanningIntervalObjectiveType.Team,
            Organization.Domain.Enums.TeamType.TeamOfTeams => PlanningIntervalObjectiveType.TeamOfTeams,
            _ => throw new ArgumentOutOfRangeException(nameof(team.Type), team.Type, null)
        };
    }
}
