using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Integrations.AzureDevOps.Models.Contracts;

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
    // TODO make this configurable
    static readonly string[] _ignoredWorkItemTypes =
        [
            "Microsoft.VSTS.WorkItemTypes.Task",
            "Microsoft.VSTS.WorkItemTypes.Issue",
            "Microsoft.VSTS.WorkItemTypes.TestCase",
            "Microsoft.VSTS.WorkItemTypes.TestPlan",
            "Microsoft.VSTS.WorkItemTypes.TestSuite"
        ];

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
            IsActive = !workItemType.IsDisabled,
        };
    }

    public static IList<IExternalWorkType> ToIExternalWorkTypes(this List<ProcessWorkItemTypeDto> workItemTypes)
    {
        // test work types typically have no behaviors
        return workItemTypes
            .Where(w => !_ignoredWorkItemTypes.Contains(w.ReferenceName)
                && (w.Inherits is null || !_ignoredWorkItemTypes.Contains(w.Inherits)))
            .Select(w => w.ToAzdoWorkType())
            .ToList<IExternalWorkType>();
    }

    public static IList<IExternalWorkStatus> ToIExternalWorkStatuses(this List<ProcessWorkItemTypeDto> workItemTypes)
    {
        return workItemTypes
            .Where(w => !_ignoredWorkItemTypes.Contains(w.ReferenceName)
                && (w.Inherits is null || !_ignoredWorkItemTypes.Contains(w.Inherits)))
            .SelectMany(w => w.States)
            .DistinctBy(s => s.Name)
            .Select(s => s.ToAzdoWorkStatus())
            .ToList<IExternalWorkStatus>();
    }

    public static List<ProcessWorkflowItemDto> ToProcessWorkflow(this List<ProcessWorkItemTypeDto> workItemTypes)
    {
        var types = workItemTypes
            .Where(w => !_ignoredWorkItemTypes.Contains(w.ReferenceName)
                && (w.Inherits is null || !_ignoredWorkItemTypes.Contains(w.Inherits)))
            .ToList();

        List<ProcessWorkflowItemDto> workflow = [];

        foreach (var type in types)
        {
            foreach (var state in type.States)
            {
                workflow.Add(new ProcessWorkflowItemDto
                {
                    TypeReferenceName = type.ReferenceName,
                    TypeName = type.Name,
                    TypeIsDisabled = type.IsDisabled,
                    BacklogLevelId = type.Behaviors.FirstOrDefault()?.Behavior.Id ?? "System.RequirementBacklogBehavior",
                    StateId = state.Id,
                    StateName = state.Name,
                    StateCategory = state.StateCategory,
                    StateOrder = state.Order,
                    StateIsDisabled = state.Hidden
                });
            }

        }

        return workflow;
    }
}
