using System.Text.Json.Serialization;

namespace Wayd.Integrations.AzureDevOps.Models.Projects;

internal record TeamSettingsBacklogIterationResponse
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }
}
