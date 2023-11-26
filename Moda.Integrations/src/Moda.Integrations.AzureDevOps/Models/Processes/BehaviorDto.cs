namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record BehaviorDto
{
    public required string ReferenceName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Rank { get; set; }
    public required string Customization { get; set; }
}
