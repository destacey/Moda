using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.Work;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using NodaTime;

namespace Moda.Integrations.AzureDevOps.Models.WorkItems;
internal sealed record ReportingWorkItemLinkResponse
{
    // TODO: Is Rel enough or shoule we use a LinkType to verify forward or reverse?
    public required string Rel { get; init; }
    public int SourceId { get; init; }
    public int TargetId { get; init; }
    public DateTime ChangedDate { get; init; }
    public UserResponse? ChangedBy { get; init; }
    public string? Comment { get; init; }
    public bool IsActive { get; set; }
    public required string ChangedOperation { get; set; }
    public Guid SourceProjectId { get; set; }
    public Guid TargetProjectId { get; set; }
}

internal static class ReportingWorkItemLinkResponseExtensions
{
    public static AzdoWorkItemLink ToAzdoWorkItemLink(this ReportingWorkItemLinkResponse workItemLink)
    {
        return new AzdoWorkItemLink()
        {
            LinkType = workItemLink.Rel switch
            {
                "System.LinkTypes.Hierarchy" => WorkItemLinkType.Hierarchy,
                "System.LinkTypes.Dependency" => WorkItemLinkType.Dependency,
                _ => (WorkItemLinkType)999 // Unknown
            },
            SourceId = workItemLink.SourceId,   // Parent/Predecessor
            TargetId = workItemLink.TargetId,   // Child/Successor            
            ChangedDate = Instant.FromDateTimeOffset(workItemLink.ChangedDate),
            ChangedBy = workItemLink.ChangedBy?.UniqueName,
            Comment = workItemLink.Comment,
            IsActive = workItemLink.IsActive,
            ChangedOperation = workItemLink.ChangedOperation,
            SourceWorkspaceId = workItemLink.SourceProjectId,
            TargetWorkspaceId = workItemLink.TargetProjectId
        };
    }

    public static List<IExternalWorkItemLink> ToIExternalWorkItemLinks(this List<ReportingWorkItemLinkResponse> workItemLinks)
    {
        var result = new List<IExternalWorkItemLink>(workItemLinks.Count);
        foreach (var workItemLink in workItemLinks)
        {
            result.Add(workItemLink.ToAzdoWorkItemLink());
        }
        return result;
    }
}
