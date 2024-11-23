using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Enums.Work;
using Moda.Common.Extensions;

namespace Moda.Work.Domain.Models;
public sealed class WorkItemLink : BaseEntity<Guid>, ISystemAuditable
{
    private string? _comment;

    private WorkItemLink() { }

    public WorkItemLink(Guid sourceId, Guid targetId, WorkItemLinkType linkType, DateTime createdOn, Guid? createdById, DateTime? endedOn, Guid? endedById, string? comment)
    {
        if (sourceId == targetId)
            throw new ArgumentException("A work item cannot be linked to itself.", nameof(targetId));        

        SourceId = sourceId;
        TargetId = targetId;
        LinkType = linkType;
        CreatedOn = createdOn;
        CreatedById = createdById;
        EndedOn = endedOn;
        EndedById = endedById;
        Comment = comment;
    }

    public Guid SourceId { get; private init; }
    
    public WorkItem? Source { get; private set; }
    
    public Guid TargetId { get; private init; }
    
    public WorkItem? Target { get; private set; }
    
    public WorkItemLinkType LinkType { get; private init; }
    
    public DateTime CreatedOn { get; private init; }

    public Guid? CreatedById { get; set; }

    public Employee? CreatedBy { get; private set; }

    public DateTime? EndedOn { get; private set; }

    public Guid? EndedById { get; set; }

    public Employee? EndedBy { get; private set; }

    public string? Comment 
    { 
        get => _comment; 
        private set => _comment = value.NullIfWhiteSpacePlusTrim(); 
    }

    public void EndLink(DateTime endedOn)
    {
        EndedOn = endedOn;
    }

    public static WorkItemLink Create(Guid sourceId, Guid targetId, WorkItemLinkType linkType, DateTime createdOn, Guid? createdById, DateTime? endedOn, Guid? endedById, string? comment)
    {
        return new WorkItemLink(sourceId, targetId, linkType, createdOn, createdById, endedOn, endedById, comment);
    }

    public static WorkItemLink CreateHierarchy(Guid sourceId, Guid targetId, DateTime createdOn, Guid? createdById, DateTime? endedOn, Guid? endedById, string? comment)
    {
        return Create(sourceId, targetId, WorkItemLinkType.Hierarchy, createdOn, createdById, endedOn, endedById, comment);
    }

    public static WorkItemLink CreateDependency(Guid sourceId, Guid targetId, DateTime createdOn, Guid? createdById, DateTime? endedOn, Guid? endedById, string? comment)
    {
        return Create(sourceId, targetId, WorkItemLinkType.Dependency, createdOn, createdById, endedOn, endedById, comment);
    }
}
