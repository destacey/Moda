namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record ProcessDto
{
    public Guid TypeId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<ProcessProjectDto>? Projects { get; set; }
}
