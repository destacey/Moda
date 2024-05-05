namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public sealed record UpdatePlanningIntervalObjectivesOrderRequest
{
    public Guid PlanningIntervalId { get; set; }
    public Dictionary<Guid, int?> Objectives { get; set; } = [];
}