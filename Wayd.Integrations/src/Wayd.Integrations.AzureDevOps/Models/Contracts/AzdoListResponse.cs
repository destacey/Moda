using System.Text.Json.Serialization;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

internal class AzdoListResponse<T>
{
    public int Count { get; set; }

    [JsonPropertyName("value")]
    public List<T> Items { get; set; } = [];
}
