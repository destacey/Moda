namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record WorkItemTypeDto
{
    public required string ReferenceName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsDisabled { get; set; }
    public List<ProcessWorkItemStateDto> States { get; set; } = new();
    public List<ProcessWorkItemTypeBehaviorsDto> Behaviors { get; set; } = new();
}

internal static class WorkItemTypeDtoExtensions
{
    public static AzdoWorkType ToAzdoWorkType(this WorkItemTypeDto workItemType)
    {
        return new AzdoWorkType
        {
            Id = workItemType.ReferenceName,
            Name = workItemType.Name,
            Description = workItemType.Description,
            IsDisabled = workItemType.IsDisabled,
        };
    }

    public static List<AzdoWorkType> ToAzdoWorkTypes(this List<WorkItemTypeDto> workItemTypes)
    {
        // test work types typically have no behaviors
        return workItemTypes
            .Where(w => !w.IsDisabled && w.Behaviors.Any())
            .Select(w => w.ToAzdoWorkType())
            .ToList();
    }
}
