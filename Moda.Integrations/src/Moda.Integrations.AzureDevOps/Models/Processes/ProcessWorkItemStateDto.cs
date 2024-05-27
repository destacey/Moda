using Moda.Common.Domain.Enums.Work;
using Moda.Integrations.AzureDevOps.Models.Contracts;

namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record ProcessWorkItemStateDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string StateCategory { get; set; }
    public int Order { get; set; }
    public bool Hidden { get; set; }
}

internal static class ProcessWorkItemStateDtoExtensions
{
    public static AzdoWorkStatus ToAzdoWorkStatus(this ProcessWorkItemStateDto workItemState)
    {
        return new AzdoWorkStatus
        {
            Name = workItemState.Name
        };
    }

    public static AzdoWorkflowState ToAzdoWorkflowState(this ProcessWorkItemStateDto workItemState)
    {
        return new AzdoWorkflowState
        {
            StatusName = workItemState.Name,
            Category = workItemState.StateCategory switch
            {
                "Proposed" => WorkStatusCategory.Proposed,
                "InProgress" => WorkStatusCategory.Active,
                "Resolved" => WorkStatusCategory.Active,
                "Completed" => WorkStatusCategory.Done,
                "Removed" => WorkStatusCategory.Removed,
                _ => throw new ArgumentOutOfRangeException(nameof(workItemState.StateCategory), workItemState.StateCategory, null)
            },
            Order = workItemState.Order,
            IsActive = !workItemState.Hidden
        };
    }
}
