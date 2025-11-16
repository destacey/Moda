using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Domain.Enums.Work;
using NodaTime;
using System.Linq.Expressions;

namespace Moda.Work.Domain.Models;
public sealed record DependencyWorkItemInfo
{
    public Guid WorkItemId { get; set; }
    public WorkStatusCategory StatusCategory { get; set; }
    public Instant? PlannedOn { get; set; }

    /// <summary>
    /// Expression for projecting a WorkItem to DependencyWorkItemInfo in EF Core queries.
    /// This can be used directly in Select() statements for efficient database queries.
    /// </summary>
    public static Expression<Func<WorkItem, DependencyWorkItemInfo>> Projection => wi => new DependencyWorkItemInfo
    {
        WorkItemId = wi.Id,
        StatusCategory = wi.StatusCategory,
        PlannedOn = wi.Iteration != null && wi.Iteration.Type == IterationType.Sprint
            ? wi.Iteration.DateRange.End
            : null
    };

    public static DependencyWorkItemInfo Create(WorkItem workItem, Instant? now = null)
    {
        Instant? plannedOn = null;
        if (workItem.Iteration != null 
            && workItem.Iteration.Type == IterationType.Sprint 
            && workItem.Iteration.State != IterationState.Completed
            && (!now.HasValue || workItem.Iteration.DateRange.End > now.Value))
        {
            plannedOn = workItem.Iteration.DateRange.End;
        }

        return new DependencyWorkItemInfo
        {
            WorkItemId = workItem.Id,
            StatusCategory = workItem.StatusCategory,
            PlannedOn = plannedOn
        };
    }
}
