using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems;
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
