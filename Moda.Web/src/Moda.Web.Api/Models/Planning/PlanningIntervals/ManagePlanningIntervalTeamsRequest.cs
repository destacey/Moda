using Moda.Planning.Application.PlanningIntervals.Commands;

namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public sealed record ManagePlanningIntervalTeamsRequest
{
    public Guid Id { get; set; }

    public IEnumerable<Guid> TeamIds { get; set; } = new List<Guid>();

    public ManagePlanningIntervalTeamsCommand ToManagePlanningIntervalTeamsCommand()
    {
        return new ManagePlanningIntervalTeamsCommand(Id, TeamIds);
    }
}
