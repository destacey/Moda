using Ardalis.GuardClauses;
using Moda.Common.Domain.Employees;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkItem : BaseEntity<Guid>
{
    private WorkItemKey _key = null!;
    private string _title = null!;

    //private readonly List<WorkItemRevision> _history = [];

    private WorkItem() { }

    private WorkItem(WorkItemKey key, string title, Guid workspaceId, int? externalId, int typeId, int statusId, Instant created, Guid? createdById, Instant lastModified, Guid? lastModifiedById, Guid? assignedToId, int priority)
    {
        Key = key;
        Title = title;
        WorkspaceId = workspaceId;
        ExternalId = externalId;
        TypeId = typeId;
        StatusId = statusId;
        Created = created;
        CreatedById = createdById;
        LastModified = lastModified;
        LastModifiedById = lastModifiedById;
        AssignedToId = assignedToId;
        Priority = priority;
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

    //public Guid BacklogLevelId { get; private set; }

    //public BacklogLevel BacklogLevel { get; private set; } = null!;

    //public Guid WorkProcessConfigurationId { get; private set; }

    //public WorkProcessScheme WorkProcessConfiguration { get; private set; } = null!;

    public Guid? AssignedToId { get; private set; }

    public Employee? AssignedTo { get; private set; }

    public Instant Created { get; private set; }

    public Guid? CreatedById { get; private set; }

    public Employee? CreatedBy { get; private set; }

    public Instant LastModified { get; private set; }

    public Guid? LastModifiedById { get; private set; }

    public Employee? LastModifiedBy { get; private set; }

    public int Priority { get; private set; }

    // TODO: saving everything as string won't work for systems that use numbers because the sorting will be incorrect.
    //public string StackRank { get; set; }  


    /// <summary>
    /// The collection of revisions for the life of the work item.
    /// </summary>
    //public IReadOnlyCollection<WorkItemRevision> History => _history.AsReadOnly();


    public static WorkItem CreateExternal(Workspace workspace, int number, string title, int? externalId, int typeId, int statusId, Instant created, Guid? createdById, Instant lastModified, Guid? lastModifiedById, Guid? assignedToId, int priority)
    {
        Guard.Against.Null(workspace, nameof(workspace));
        if (workspace.Ownership != Ownership.Owned)
        {
            throw new InvalidOperationException("Owned work items cannot have an external id.");
        }
        else if (workspace.Ownership != Ownership.Owned && !externalId.HasValue)
        {
            throw new InvalidOperationException("External work items must have an external id.");
        }
        
        var key = new WorkItemKey(workspace.Key, number);
        var workItem = new WorkItem(key, title, workspace.Id, externalId, typeId, statusId, created, createdById, lastModified, lastModifiedById, assignedToId, priority);

        var result = workspace.AddWorkItem(workItem);
        return result.IsSuccess ? workItem : throw new InvalidOperationException(result.Error);
    }
}
