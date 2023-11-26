namespace Moda.Integrations.AzureDevOps.Models.Processes;
internal sealed record GetProcessesResponse
{
    public int Count { get; set; }
    public List<ProcessDto> Value { get; set; } = new();

    public List<AzdoWorkProcess> ToAzdoWorkProcesses()
    {
        return Value.Select(p => new AzdoWorkProcess
        {
            Id = p.TypeId,
            Name = p.Name,
            Description = p.Description,
            WorkspaceIds = p.Projects?.Select(p => p.Id).ToList() ?? new List<Guid>()
        }).ToList();
    }
}