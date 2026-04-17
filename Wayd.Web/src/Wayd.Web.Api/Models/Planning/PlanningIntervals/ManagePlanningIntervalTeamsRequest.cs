using Wayd.Planning.Application.PlanningIntervals.Commands;

namespace Wayd.Web.Api.Models.Planning.PlanningIntervals;

public sealed record ManagePlanningIntervalTeamsRequest
{
    /// <summary>
    /// The ID of the planning interval to manage teams for.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The list of team IDs to be associated with the planning interval.
    /// </summary>
    public List<Guid> TeamIds { get; set; } = [];

    public ManagePlanningIntervalTeamsCommand ToManagePlanningIntervalTeamsCommand()
    {
        return new ManagePlanningIntervalTeamsCommand(Id, TeamIds);
    }
}
