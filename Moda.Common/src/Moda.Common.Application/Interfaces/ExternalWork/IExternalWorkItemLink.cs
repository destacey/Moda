using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkItemLink
{
    WorkItemLinkType LinkType { get; set; }
    int SourceId { get; set; }
    int TargetId { get; set; }
    Instant ChangedDate { get; set; }
    string? ChangedBy { get; set; }
    string? Comment { get; set; }
    bool IsActive { get; set; }
    string ChangedOperation { get; set; }
    Guid SourceWorkspaceId { get; set; }
    Guid TargetWorkspaceId { get; set; }
}
