namespace Moda.Planning.Domain.Models;
public class PlanningIntervalTeam
{
    private PlanningIntervalTeam() { }
    internal PlanningIntervalTeam(Guid planningIntervalId, Guid teamId)
    {
        PlanningIntervalId = planningIntervalId;
        TeamId = teamId;
    }

    public Guid PlanningIntervalId { get; private set; }
    public Guid TeamId { get; private set; }
    public PlanningTeam Team { get; private set; } = default!;
}
