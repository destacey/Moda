using System.Text.Json.Serialization;
using Moda.Common.Application.Interfaces.Work;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using Moda.Integrations.AzureDevOps.Models.Projects;
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
    private static readonly double _defaultStackRank = 999_999_999_999D;

    public static AzdoWorkItem ToAzdoWorkItem(this WorkItemResponse workItem, IterationDto iteration)
    {
        var created = Instant.FromDateTimeOffset(workItem.Fields.CreatedDate);
        Instant? activated = workItem.Fields.ActivatedDate.HasValue ? Instant.FromDateTimeUtc(workItem.Fields.ActivatedDate.Value) : null;
        Instant? closed = workItem.Fields.ClosedDate.HasValue ? Instant.FromDateTimeUtc(workItem.Fields.ClosedDate.Value) : null;

        return new AzdoWorkItem()
        {
            Id = workItem.Id,
            Title = workItem.Fields.Title,
            WorkType = workItem.Fields.WorkItemType,
            WorkStatus = workItem.Fields.State,
            ParentId = workItem.Fields.Parent,
            AssignedTo = workItem.Fields.AssignedTo?.UniqueName,
            Created = Instant.FromDateTimeOffset(workItem.Fields.CreatedDate),
            CreatedBy = workItem.Fields.CreatedBy?.UniqueName,
            LastModified = Instant.FromDateTimeOffset(workItem.Fields.ChangedDate),
            LastModifiedBy = workItem.Fields.ChangedBy?.UniqueName,
            Priority = workItem.Fields.Priority,
            StackRank = workItem.Fields.StackRank > 0 ? workItem.Fields.StackRank : _defaultStackRank,
            ActivatedTimestamp = activated.HasValue 
                ? activated < created ? created : activated
                : null,
            DoneTimestamp = closed.HasValue 
                ? closed < created ? created : closed
                : null,
            TeamId = iteration.TeamId,
            ExternalTeamIdentifier = iteration.Identifier.ToString()
        };
    }

    public static List<IExternalWorkItem> ToIExternalWorkItems(this List<WorkItemResponse> workItems, List<IterationDto> iterations)
    {
        var iterationsDictionary = iterations.ToDictionary(x => x.Id, x => x);
        var result = new List<IExternalWorkItem>(workItems.Count);
        foreach (var workItem in workItems)
        {
            var iteration = iterationsDictionary[workItem.Fields.IterationId];
            result.Add(workItem.ToAzdoWorkItem(iteration));
        }
        return result;
    }
}
