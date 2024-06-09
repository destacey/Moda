using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models.WorkItems;
internal class WorkItemFieldsResponse
{
    [JsonPropertyName("System.Title")]
    public required string Title { get; set; }

    [JsonPropertyName("System.WorkItemType")]
    public required string WorkItemType { get; set; }

    [JsonPropertyName("System.State")]
    public required string State { get; set; }

    [JsonPropertyName("System.Parent")]
    public int? Parent { get; set; }

    [JsonPropertyName("System.CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("System.CreatedBy")]
    public required UserResponse CreatedBy { get; set; }

    [JsonPropertyName("System.ChangedDate")]
    public DateTime ChangedDate { get; set; }

    [JsonPropertyName("System.ChangedBy")]
    public required UserResponse ChangedBy { get; set; }

    [JsonPropertyName("Microsoft.VSTS.Common.Priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("Microsoft.VSTS.Common.StackRank")]
    public double StackRank { get; set; }

    [JsonPropertyName("System.AssignedTo")]
    public UserResponse? AssignedTo { get; set; }

    [JsonPropertyName("System.AreaId")]
    public int AreaId { get; set; }
}
