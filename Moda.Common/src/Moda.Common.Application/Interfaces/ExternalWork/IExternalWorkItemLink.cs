using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkItemLink
{
    WorkItemLinkType LinkType { get; set; }
    int SourceId { get; set; }
    int TargetId { get; set; }
    DateTime ChangedDate { get; set; }
    bool IsActive { get; set; }
    string ChangedOperation { get; set; }
    Guid SourceProjectId { get; set; }
    Guid TargetProjectId { get; set; }
}
