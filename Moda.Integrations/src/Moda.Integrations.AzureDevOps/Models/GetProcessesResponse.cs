namespace Moda.Integrations.AzureDevOps.Models;
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

internal sealed record ProcessDto
{
    public Guid TypeId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<ProcessProjectDto>? Projects { get; set; }
}

internal sealed record ProcessProjectDto
{
    public Guid Id { get; set; }
}
