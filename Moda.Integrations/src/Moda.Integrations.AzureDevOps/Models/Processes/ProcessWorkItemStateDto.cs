namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record ProcessWorkItemStateDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string StateCategory { get; set; }
    public int Order { get; set; }
    public bool Hidden { get; set; }
}
