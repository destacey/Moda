using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Enums.Work;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

public abstract class WorkItemLink : BaseEntity<Guid>, ISystemAuditable
{
    private string? _comment;

    // EF
    protected WorkItemLink() { }

    protected WorkItemLink(Guid sourceId, Guid targetId, WorkItemLinkType linkType, Instant createdOn, Guid? createdById, Instant? removedOn, Guid? removedById, string? comment)
    {
        if (sourceId == targetId)
            throw new ArgumentException("A work item cannot be linked to itself.", nameof(targetId));        

        SourceId = sourceId;
        TargetId = targetId;
        LinkType = linkType;
        CreatedOn = createdOn;
        CreatedById = createdById;
        RemovedOn = removedOn;
        RemovedById = removedById;
        Comment = comment;
    }

    public Guid SourceId { get; private init; }
    
    public WorkItem? Source { get; private set; }
    
    public Guid TargetId { get; private init; }
    
    public WorkItem? Target { get; private set; }
    
    public WorkItemLinkType LinkType { get; private set; }
    
    public Instant CreatedOn { get; private init; }

    public Guid? CreatedById { get; set; }

    public Employee? CreatedBy { get; private set; }

    public Instant? RemovedOn { get; private set; }

    public Guid? RemovedById { get; set; }

    public Employee? RemovedBy { get; private set; }

    public string? Comment 
    { 
        get => _comment; 
        private set => _comment = value.NullIfWhiteSpacePlusTrim(); 
    }

    public void Update(Guid? createdById, Guid? removedById, string? comment)
    {
        CreatedById = createdById;
        RemovedById = removedById;
        Comment = comment;
    }

    public void RemoveLink(Instant removedOn, Guid? removedById)
    {
        RemovedOn = removedOn;
        RemovedById = removedById;
    }
}

public sealed class WorkItemHierarchy : WorkItemLink
{
    private WorkItemHierarchy() : base() { }

    public WorkItemHierarchy(Guid sourceId, Guid targetId, Instant createdOn, Guid? createdById, Instant? removedOn, Guid? removedById, string? comment)
    : base(sourceId, targetId, WorkItemLinkType.Hierarchy, createdOn, createdById, removedOn, removedById, comment)
    {
    }

    public static WorkItemHierarchy Create(Guid sourceId, Guid targetId, Instant createdOn, Guid? createdById, Instant? removedOn, Guid? removedById, string? comment)
    {
        return new WorkItemHierarchy(sourceId, targetId, createdOn, createdById, removedOn, removedById, comment);
    }
}

public sealed class WorkItemDependency : WorkItemLink
{
    private WorkItemDependency() : base() { }

    public WorkItemDependency(Guid sourceId, Guid targetId, Instant createdOn, Guid? createdById, Instant? removedOn, Guid? removedById, string? comment)
    : base(sourceId, targetId, WorkItemLinkType.Dependency, createdOn, createdById, removedOn, removedById, comment)
    {
    }

    public static WorkItemDependency Create(Guid sourceId, Guid targetId, Instant createdOn, Guid? createdById, Instant? removedOn, Guid? removedById, string? comment)
    {
        return new WorkItemDependency(sourceId, targetId, createdOn, createdById, removedOn, removedById, comment);
    }
}
