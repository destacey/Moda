using Wayd.Work.Application.WorkItems.Dtos;

namespace Wayd.Work.Application.WorkItems;

internal sealed class WorkItemProgressSummary
{
    public required WorkItemProgressRollupDto RootRollup { get; set; }

    public static WorkItemProgressSummary Create(List<WorkItemProgressStateDto> workItems)
    {
        return new WorkItemProgressSummary
        {
            RootRollup = WorkItemProgressRollupDto.Create(workItems)
        };
    }
}
