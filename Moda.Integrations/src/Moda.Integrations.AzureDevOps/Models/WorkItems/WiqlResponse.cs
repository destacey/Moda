namespace Moda.Integrations.AzureDevOps.Models.WorkItems;
internal sealed record WiqlResponse
{
    public required string QueryType { get; set; }
    public required string QueryResultType { get; set; }
    public DateTime? AsOf { get; set; }
    public required IList<WiqlWorkItemResponse> WorkItems { get; set; } = [];
}
