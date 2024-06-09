using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models;
internal record ClassificationNodeDto
{
    //public int Id { get; set; }
    public Guid Identifier { get; set; }
    public required string Name { get; set; }
    public bool? HasChildren { get; set; }
    public List<ClassificationNodeDto> Children { get; set; } = [];
    public required string Path { get; set; }

    [JsonIgnore]
    public string WorkItemPath => Path[1..].Replace("\\Area", "");
}
