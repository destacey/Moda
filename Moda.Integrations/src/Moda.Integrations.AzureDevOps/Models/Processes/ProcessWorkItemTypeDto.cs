namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record ProcessWorkItemTypeDto
{
    public required string ReferenceName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Inherits { get; set; }
    public bool IsDisabled { get; set; }
    public List<ProcessWorkItemStateDto> States { get; set; } = [];
    public List<ProcessWorkItemTypeBehaviorsDto> Behaviors { get; set; } = [];
}

internal static class ProcessWorkItemTypeDtoExtensions
{
    public static AzdoWorkType ToAzdoWorkType(this ProcessWorkItemTypeDto workItemType)
    {
        // TODO make this configurable
        var backlogLevelId = workItemType.Behaviors.FirstOrDefault()?.Behavior.Id ?? "System.RequirementBacklogBehavior";

        return new AzdoWorkType
        {
            Id = workItemType.ReferenceName,
            Name = workItemType.Name,
            Description = workItemType.Description,
            BacklogLevelId = backlogLevelId,
            IsDisabled = workItemType.IsDisabled,
        };
    }

    public static List<AzdoWorkType> ToAzdoWorkTypes(this List<ProcessWorkItemTypeDto> workItemTypes)
    {
        // TODO make this configurable
        var ignoredWorkItemTypes = new List<string>
        {
            "Microsoft.VSTS.WorkItemTypes.Task",
            "Microsoft.VSTS.WorkItemTypes.Issue",
            "Microsoft.VSTS.WorkItemTypes.TestCase",
            "Microsoft.VSTS.WorkItemTypes.TestPlan",
            "Microsoft.VSTS.WorkItemTypes.TestSuite"
        };

        // test work types typically have no behaviors
        return workItemTypes
            .Where(w => !w.IsDisabled 
                && !ignoredWorkItemTypes.Contains(w.ReferenceName)
                && (w.Inherits is null || !ignoredWorkItemTypes.Contains(w.Inherits)))
            .Select(w => w.ToAzdoWorkType())
            .ToList();
    }
}
