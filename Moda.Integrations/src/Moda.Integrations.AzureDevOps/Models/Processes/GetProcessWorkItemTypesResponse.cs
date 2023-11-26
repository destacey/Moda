namespace Moda.Integrations.AzureDevOps.Models.Processes;
internal sealed record GetProcessWorkItemTypesResponse
{
    public int Count { get; set; }
    public List<ProcessDto> Value { get; set; } = new();
}