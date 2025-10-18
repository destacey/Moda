using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models;
internal record ConnectionDataResponse
{
    [JsonPropertyName("instanceId")]
    public required string InstanceId { get; set; }
}
