using Ardalis.GuardClauses;
using Moda.Common.Domain.Enums.Work;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkItemDependency : WorkItemLink
{
    private WorkItemDependency() : base() { }

    private WorkItemDependency(Guid sourceId, Guid targetId, WorkStatusCategory sourceStatusCategory, Instant? sourcePlannedDate, Instant? targetPlannedDate, Instant createdOn, Guid? createdById, Instant? removedOn, Guid? removedById, string? comment, Instant now)
    : base(sourceId, targetId, WorkItemLinkType.Dependency, createdOn, createdById, removedOn, removedById, comment)
    {
        // assign inputs to properties first (important for state calculation)
        SourceWorkStatusCategory = sourceStatusCategory;
        SourcePlannedOn = sourcePlannedDate;
        TargetPlannedOn = targetPlannedDate;

        // compute derived values
        State = ComputeState(SourceWorkStatusCategory, removedOn);
        Health = ComputeHealth(SourcePlannedOn, TargetPlannedOn, State, now);
    }

    public WorkStatusCategory SourceWorkStatusCategory { get; set; }

    public Instant? SourcePlannedOn { get; set; }

    public Instant? TargetPlannedOn { get; set; }

    public DependencyState State { get; private set; }

    public DependencyPlanningHealth Health { get; private set; }

    /// <summary>
    /// Updates the source status category and planned date, and recalculates the derived state and health values based
    /// on the provided information.
    /// </summary>
    /// <param name="sourceStatusCategory">The new status category to assign to the source. Determines the updated state of the source.</param>
    /// <param name="sourcePlannedOn">The planned date and time for the source, or null if no planned date is set.</param>
    /// <param name="now">The current point in time used for recalculating derived values such as health.</param>
    public void UpdateSourceDetails(WorkStatusCategory sourceStatusCategory, Instant? sourcePlannedOn, Instant now)
    {
        SourceWorkStatusCategory = sourceStatusCategory;
        SourcePlannedOn = sourcePlannedOn;
        
        // recompute derived values
        State = ComputeState(SourceWorkStatusCategory, RemovedOn);
        Health = ComputeHealth(SourcePlannedOn, TargetPlannedOn, State, now);
    }

    /// <summary>
    /// Updates the target planned date and recalculates related health status based on the specified current time.
    /// </summary>
    /// <param name="targetPlannedOn">The new target planned date and time to set, or null to clear the target planned date.</param>
    /// <param name="now">The current point in time used to recalculate health status.</param>
    public void UpdateTargetPlannedDate(Instant? targetPlannedOn, Instant now)
    {
        TargetPlannedOn = targetPlannedOn;

        // recompute derived values
        Health = ComputeHealth(SourcePlannedOn, TargetPlannedOn, State, now);
    }

    public static WorkItemDependency Create(Guid sourceId, Guid targetId, WorkStatusCategory sourceStatusCategory, Instant? sourcePlannedOn, Instant? targetPlannedOn, Instant createdOn, Guid? createdById, Instant? removedOn, Guid? removedById, string? comment, Instant now)
    {
        Guard.Against.NullOrEmpty(sourceId);
        Guard.Against.NullOrEmpty(targetId);

        return new WorkItemDependency(sourceId, targetId, sourceStatusCategory, sourcePlannedOn, targetPlannedOn, createdOn, createdById, removedOn, removedById, comment, now);
    }

    private static DependencyState ComputeState(WorkStatusCategory sourceStatusCategory, Instant? removedOn)
    {
        if (removedOn.HasValue)
            return DependencyState.Removed;

        return sourceStatusCategory switch
        {
            WorkStatusCategory.Proposed => DependencyState.ToDo,
            WorkStatusCategory.Active => DependencyState.InProgress,
            WorkStatusCategory.Done => DependencyState.Done,
            WorkStatusCategory.Removed => DependencyState.Removed,
            _ => throw new ArgumentOutOfRangeException(nameof(sourceStatusCategory), sourceStatusCategory, null)
        };
    }

    /// <summary>
    /// Computes the planning health of the dependency based on the planned dates of the source and target,
    /// </summary>
    /// <param name="sourcePlanned"></param>
    /// <param name="targetPlanned"></param>
    /// <param name="state"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    private static DependencyPlanningHealth ComputeHealth(Instant? sourcePlanned, Instant? targetPlanned, DependencyState state, Instant now)
    {
        if (state == DependencyState.Done)
            return DependencyPlanningHealth.Healthy;

        if (state == DependencyState.Removed)
            return DependencyPlanningHealth.Unhealthy;

        // Treat past-or-equal planned dates as unplanned
        var predecessor = (sourcePlanned.HasValue && sourcePlanned.Value <= now) ? null : sourcePlanned;
        var successor = (targetPlanned.HasValue && targetPlanned.Value <= now) ? null : targetPlanned;

        // neither side planned -> at risk
        if (predecessor == null && successor == null)
            return DependencyPlanningHealth.AtRisk;

        // predecessor planned and successor unplanned -> healthy
        if (predecessor != null && successor == null)
            return DependencyPlanningHealth.Healthy;

        // if both planned and predecessor is on or before successor -> healthy
        if (predecessor != null && successor != null && predecessor <= successor)
            return DependencyPlanningHealth.Healthy;

        // otherwise predecessor planned after successor -> unhealthy
        return DependencyPlanningHealth.Unhealthy;
    }
}
