using System.Text.Json.Serialization;

namespace Wayd.Integrations.AzureDevOps.Models;

internal record ConnectionDataResponse
{
    [JsonPropertyName("instanceId")]
    public required string InstanceId { get; set; }
}
