namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemsSummaryDto
{
    public required WorkItemProgressRollupDto ProgressSummary { get; set; }
    public required IReadOnlyCollection<WorkItemListDto> WorkItems { get; set; }

    public static WorkItemsSummaryDto Create(WorkItemProgressRollupDto progressSummary, IReadOnlyCollection<WorkItemListDto> workItems)
    {
        return new WorkItemsSummaryDto
        {
            ProgressSummary = progressSummary,
            WorkItems = workItems
        };
    }
}
