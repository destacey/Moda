namespace Moda.Integrations.AzureDevOps.Models;
internal sealed record BatchResponse<T>
{
    public List<T> Values { get; set; } = [];
    public bool IsLastBatch { get; set; }
    public string? ContinuationToken { get; set; }
    public string? NextLink { get; set; }
}
