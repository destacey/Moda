using System.Text.Json.Serialization;
using Moda.Common.Application.Interfaces.Work;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using NodaTime;

namespace Moda.Integrations.AzureDevOps.Models.WorkItems;
internal class WorkItemResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("rev")]
    public int? Rev { get; set; }

    [JsonPropertyName("fields")]
    public required WorkItemFieldsResponse Fields { get; set; }
}

internal static class WorkItemResponseExtensions
{
    public static AzdoWorkItem ToAzdoWorkItem(this WorkItemResponse workItem)
    {
        return new AzdoWorkItem()
        {
            Id = workItem.Id,
            Title = workItem.Fields.Title,
            WorkType = workItem.Fields.WorkItemType,
            WorkStatus = workItem.Fields.State,
            AssignedTo = workItem.Fields.AssignedTo?.UniqueName,
            Created = Instant.FromDateTimeOffset(workItem.Fields.CreatedDate),
            CreatedBy = workItem.Fields.CreatedBy.UniqueName,
            LastModified = Instant.FromDateTimeOffset(workItem.Fields.ChangedDate),
            LastModifiedBy = workItem.Fields.ChangedBy.UniqueName,
            Priority = workItem.Fields.Priority,
            StackRank = workItem.Fields.StackRank
        };
    }

    public static List<IExternalWorkItem> ToIExternalWorkItems(this List<WorkItemResponse> workItems)
    {
        var result = new List<IExternalWorkItem>(workItems.Count);
        foreach (var workItem in workItems)
        {
            result.Add(workItem.ToAzdoWorkItem());
        }
        return result;
    }
}
