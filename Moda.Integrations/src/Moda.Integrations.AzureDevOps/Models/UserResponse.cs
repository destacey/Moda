using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models;
internal sealed record UserResponse
{
    //[JsonPropertyName("id")]
    //public required string Id { get; set; }

    //[JsonPropertyName("displayName")]
    //public required string DisplayName { get; set; }

    [JsonPropertyName("uniqueName")]
    public string? UniqueName { get; set; }

    //[JsonPropertyName("descriptor")]
    //public string? Descriptor { get; set; }
}
