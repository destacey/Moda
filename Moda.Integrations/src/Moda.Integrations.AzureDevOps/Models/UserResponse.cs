using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models;
internal sealed record UserResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }


    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }


    [JsonPropertyName("uniqueName")]
    public required string UniqueName { get; set; }
}
