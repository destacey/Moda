using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.Work;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkItemLink : IExternalWorkItemLink
{
    public WorkItemLinkType LinkType { get; set; }
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public DateTime ChangedDate { get; set; }
    public bool IsActive { get; set; }
    public required string ChangedOperation { get; set; }
    public Guid SourceProjectId { get; set; }
    public Guid TargetProjectId { get; set; }
}
