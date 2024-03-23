namespace Moda.Integrations.AzureDevOps.Models;
internal sealed record ListResponse<T>
{
    public int Count { get; set; }
    public List<T> Value { get; set; } = [];
}
