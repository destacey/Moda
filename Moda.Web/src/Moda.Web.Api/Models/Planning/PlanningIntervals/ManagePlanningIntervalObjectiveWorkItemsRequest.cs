using Moda.Common.Domain.Enums;
using Moda.Work.Application.WorkItems.Commands;

namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public sealed record ManagePlanningIntervalObjectiveWorkItemsRequest
{
    public Guid PlanningIntervalId { get; set; }
    public Guid ObjectiveId { get; set; }
    public IEnumerable<Guid> WorkItemIds { get; set; } = [];

    public ManageExternalObjectWorkItemsCommand ToManageExternalObjectWorkItemsCommand()
    {
        return new ManageExternalObjectWorkItemsCommand(PlanningIntervalId, ObjectiveId, SystemContext.PlanningPlanningIntervalObjective, WorkItemIds);
    }
}
