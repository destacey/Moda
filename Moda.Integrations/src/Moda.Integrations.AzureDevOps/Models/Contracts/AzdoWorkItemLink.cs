using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.Work;
using NodaTime;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkItemLink : IExternalWorkItemLink
{
    public WorkItemLinkType LinkType { get; set; }
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public Instant ChangedDate { get; set; }
    public string? ChangedBy { get; set; }
    public string? Comment { get; set; }
    public bool IsActive { get; set; }
    public required string ChangedOperation { get; set; }
    public Guid SourceWorkspaceId { get; set; }
    public Guid TargetWorkspaceId { get; set; }
}
