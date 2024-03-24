namespace Moda.Integrations.AzureDevOps.Models;
internal sealed record PropertyDto
{
    public required string Name { get; set; }
    public required string Value { get; set; }
}
