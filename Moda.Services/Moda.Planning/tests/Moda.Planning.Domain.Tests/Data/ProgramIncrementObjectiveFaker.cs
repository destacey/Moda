using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models;

namespace Moda.Planning.Domain.Tests.Data;
public class ProgramIncrementObjectiveFaker : Faker<ProgramIncrementObjective>
{
    public ProgramIncrementObjectiveFaker(Guid programIncrementId, PlanningTeam team, ObjectiveStatus status, bool isStretch)
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.ProgramIncrementId, programIncrementId);
        RuleFor(x => x.TeamId, team.Id);
        RuleFor(x => x.Team, team);
        RuleFor(x => x.ObjectiveId, f => f.Random.Guid());        
        RuleFor(x => x.Type, SetType(team));
        RuleFor(x => x.Status, status);
        RuleFor(x => x.IsStretch, isStretch);
    }

    private static ProgramIncrementObjectiveType SetType(PlanningTeam team)
    {
        return team.Type switch
        {
            Organization.Domain.Enums.TeamType.Team => ProgramIncrementObjectiveType.Team,
            Organization.Domain.Enums.TeamType.TeamOfTeams => ProgramIncrementObjectiveType.TeamOfTeams,
            _ => throw new ArgumentOutOfRangeException(nameof(team.Type), team.Type, null)
        };
    }
}
