namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record ProcessDto
{
    public Guid TypeId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public List<ProcessProjectDto>? Projects { get; set; }
}

internal static class ProcessDtoExtensions
{
    public static AzdoWorkProcess ToAzdoWorkProcess(this ProcessDto process)
    {
        return new AzdoWorkProcess
        {
            Id = process.TypeId,
            Name = process.Name,
            Description = process.Description,
            IsEnabled = process.IsEnabled,
            WorkspaceIds = process.Projects?.Select(p => p.Id).ToList() ?? [],
        };
    }

    public static AzdoWorkProcessConfiguration ToAzdoWorkProcessDetails(this ProcessDto process, List<BehaviorDto> behaviors, List<ProcessWorkItemTypeDto> workTypes)
    {
        return new AzdoWorkProcessConfiguration
        {
            Id = process.TypeId,
            Name = process.Name,
            Description = process.Description,
            IsEnabled = process.IsEnabled,
            WorkspaceIds = process.Projects?.Select(p => p.Id).ToList() ?? [],
            BacklogLevels = behaviors.ToAzdoBacklogLevels(),
            WorkTypes = workTypes.ToAzdoWorkTypes(),
        };
    }

    public static List<AzdoWorkProcess> ToAzdoWorkProcesses(this List<ProcessDto> processes)
    {
        return processes.Select(p => p.ToAzdoWorkProcess()).ToList();
    }
}
