using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models;
internal record IterationNodeResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("identifier")]
    public Guid Identifier { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("path")]
    public required string Path { get; set; }

    [JsonPropertyName("attributes")]
    public IterationAttributes? Attributes { get; set; }

    [JsonPropertyName("children")]
    public List<IterationNodeResponse>? Children { get; set; }

    //[JsonIgnore]
    //public string WorkItemPath => Path[1..].Replace("\\Iteration", "");
}

internal record IterationAttributes
{
    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("finishDate")]
    public DateTime? EndDate { get; set; }
}
