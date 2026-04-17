using System.Text.Json.Serialization;

namespace Wayd.Integrations.AzureDevOps.Models.Projects;

internal record TeamSettingsResponse
{
    [JsonPropertyName("backlogIteration")]
    public TeamSettingsBacklogIterationResponse? BacklogIteration { get; set; }
}
