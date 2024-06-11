using Ardalis.GuardClauses;
using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Enums.Work;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkItem : BaseEntity<Guid>, ISystemAuditable
{
    private WorkItemKey _key = null!;
    private string _title = null!;
    private readonly List<WorkItem> _children = [];
    private readonly List<WorkItemLink> _systemLinks = [];

    //private readonly List<WorkItemRevision> _history = [];

    private WorkItem() { }

    private WorkItem(WorkItemKey key, string title, Guid workspaceId, int? externalId, int typeId, int statusId, WorkStatusCategory statusCategory, Guid? parentId, Instant created, Guid? createdById, Instant lastModified, Guid? lastModifiedById, Guid? assignedToId, int? priority, double stackRank, Instant? doneTimestamp, WorkItemExtended? extendedProps)
    {
        Key = key;
        Title = title;
        WorkspaceId = workspaceId;
        ExternalId = externalId;
        TypeId = typeId;
        StatusId = statusId;
        StatusCategory = statusCategory;
        ParentId = parentId;
        Created = created;
        CreatedById = createdById;
        LastModified = lastModified;
        LastModifiedById = lastModifiedById;
        AssignedToId = assignedToId;
        Priority = priority;
        StackRank = stackRank;
        DoneTimestamp = doneTimestamp;
        ExtendedProps = extendedProps;
    }

    /// <summary>A unique key to identify the work item.</summary>
    /// <value>The key.</value>
    public WorkItemKey Key
    {
        get => _key;
        private set => _key = Guard.Against.Null(value, nameof(Key));
    }

    public string Title 
    { 
        get => _title; 
        private set => _title = Guard.Against.NullOrWhiteSpace(value, nameof(Title)).Trim();

    }

    public Guid WorkspaceId { get; private init; }

    public Workspace Workspace { get; private set; } = null!;

    public int? ExternalId { get; private init; }

    public int TypeId { get; private set; }

    public WorkType Type { get; private set; } = null!;

    public int StatusId { get; private set; }

    public WorkStatus Status { get; private set; } = null!;

    public WorkStatusCategory StatusCategory { get; private set; }

    public Guid? ParentId { get; private set; }

    public WorkItem? Parent { get; private set; }

    public IReadOnlyCollection<WorkItem> Children => _children.AsReadOnly();

    public Guid? AssignedToId { get; private set; }

    public Employee? AssignedTo { get; private set; }

    public Instant Created { get; private set; }

    public Guid? CreatedById { get; private set; }

    public Employee? CreatedBy { get; private set; }

    public Instant LastModified { get; private set; }

    public Guid? LastModifiedById { get; private set; }

    public Employee? LastModifiedBy { get; private set; }

    public int? Priority { get; private set; }

    // TODO: other systems will use different types.  How to handle this?
    public double StackRank { get; private set; }

    public Instant? DoneTimestamp { get; private set; }

    public WorkItemExtended? ExtendedProps { get; private set; }

    public IReadOnlyCollection<WorkItemLink> SystemLinks => _systemLinks.AsReadOnly();

    /// <summary>
    /// The collection of revisions for the life of the work item.
    /// </summary>
    //public IReadOnlyCollection<WorkItemRevision> History => _history.AsReadOnly();

    public void Update(string title, int typeId, int statusId, WorkStatusCategory statusCategory, Guid? parentId, Instant lastModified, Guid? lastModifiedById, Guid? assignedToId, int? priority, double stackRank, Instant? doneTimestamp, WorkItemExtended? extendedProps)
    {
        if (extendedProps != null && Id != extendedProps.Id)
        {
            throw new InvalidOperationException("The extended properties must match the work item.");
        }

        if (doneTimestamp.HasValue && doneTimestamp < Created)
        {
            throw new InvalidOperationException("The completed timestamp cannot be before the created timestamp.");
        }

        Title = title;
        TypeId = typeId;
        StatusId = statusId;
        StatusCategory = statusCategory;
        ParentId = parentId;
        LastModified = lastModified;
        LastModifiedById = lastModifiedById;
        AssignedToId = assignedToId;
        Priority = priority;
        StackRank = stackRank;
        DoneTimestamp = doneTimestamp.HasValue && statusCategory != WorkStatusCategory.Done ? null : doneTimestamp;

        if (extendedProps != null)
        {
            ExtendedProps = ExtendedProps ?? extendedProps;
            ExtendedProps.Update(extendedProps);
        }
        else
        {
            ExtendedProps = null;
        }
    }

    public void UpdateParent(Guid? parentId)
    {
        ParentId = parentId;
    }

    public static WorkItem CreateExternal(Workspace workspace, int externalId, string title, int typeId, int statusId, WorkStatusCategory statusCategory, Guid? parentId, Instant created, Guid? createdById, Instant lastModified, Guid? lastModifiedById, Guid? assignedToId, int? priority, double stackRank, Instant? doneTimestamp, WorkItemExtended? extendedProps)
    {
        Guard.Against.Null(workspace, nameof(workspace));
        if (workspace.Ownership != Ownership.Managed)
        {
            throw new InvalidOperationException("Only managed workspaces can have external work items.");
        }
        
        var key = new WorkItemKey(workspace.Key, externalId);
        return new WorkItem(key, title, workspace.Id, externalId, typeId, statusId, statusCategory, parentId, created, createdById, lastModified, lastModifiedById, assignedToId, priority, stackRank, doneTimestamp, extendedProps);

        //var result = workspace.AddWorkItem(workItem);  // this is handled in the handler for performance reasons
    }
}
