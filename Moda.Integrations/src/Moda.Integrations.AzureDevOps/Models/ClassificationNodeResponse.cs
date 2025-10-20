using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models;
internal record ClassificationNodeResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("identifier")]
    public Guid Identifier { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    //[JsonPropertyName("hasChildren")]
    //public bool? HasChildren { get; set; }

    [JsonPropertyName("children")]
    public List<ClassificationNodeResponse>? Children { get; set; }

    [JsonPropertyName("path")]
    public required string Path { get; set; }
}
