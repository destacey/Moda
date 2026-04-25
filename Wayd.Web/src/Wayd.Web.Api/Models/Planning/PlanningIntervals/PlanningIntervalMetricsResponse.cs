namespace Wayd.Web.Api.Models.Planning.PlanningIntervals;

/// <summary>
/// Response containing PI-level metrics aggregated across all mapped sprints in
/// every iteration of the Planning Interval. Intended as a thin, extensible
/// rollup; per-iteration breakdowns remain on the iteration-level endpoint.
/// </summary>
public sealed record PlanningIntervalMetricsResponse
{
    public Guid PlanningIntervalId { get; init; }
    public int PlanningIntervalKey { get; init; }
    public required string PlanningIntervalName { get; init; }

    public int TeamCount { get; init; }
    public int SprintCount { get; init; }

    public double? AverageCycleTimeDays { get; init; }
}
