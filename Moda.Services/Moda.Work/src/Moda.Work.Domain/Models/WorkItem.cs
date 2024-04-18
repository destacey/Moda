using Ardalis.GuardClauses;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkItem : BaseEntity<Guid>
{
    private WorkItemKey _key = null!;
    private string _title = null!;

    //private readonly List<WorkItemRevision> _history = [];

    private WorkItem() { }

    private WorkItem(WorkItemKey key, string title, Guid workspaceId, int typeId, int statusId, Instant created, Guid? createdBy, Instant lastModified, Guid? lastModifiedBy, int priority)
    {
        Key = key;
        Title = title;
        WorkspaceId = workspaceId;
        TypeId = typeId;
        StatusId = statusId;
        Created = created;
        CreatedBy = createdBy;
        LastModified = lastModified;
        LastModifiedBy = lastModifiedBy;
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

    public int TypeId { get; private set; }

    public WorkType Type { get; private set; } = null!;

    public int StatusId { get; private set; }

    public WorkStatus Status { get; private set; } = null!;

    //public Guid BacklogLevelId { get; private set; }

    //public BacklogLevel BacklogLevel { get; private set; } = null!;

    //public Guid WorkProcessConfigurationId { get; private set; }

    //public WorkProcessScheme WorkProcessConfiguration { get; private set; } = null!;

    public Instant Created { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public Instant LastModified { get; private set; }

    public Guid? LastModifiedBy { get; private set; }

    public int Priority { get; private set; }

    // TODO: saving everything as string won't work for systems that use numbers because the sorting will be incorrect.
    //public string StackRank { get; set; }  


    /// <summary>
    /// The collection of revisions for the life of the work item.
    /// </summary>
    //public IReadOnlyCollection<WorkItemRevision> History => _history.AsReadOnly();


    public static WorkItem CreateExternal(Workspace workspace, int number, string title, int typeId, int statusId, Instant created, Guid? createdBy, Instant lastModified, Guid? lastModifiedBy, int priority)
    {
        var key = new WorkItemKey(workspace.Key, number);
        var workItem = new WorkItem(key, title, workspace.Id, typeId, statusId, created, createdBy, lastModified, lastModifiedBy, priority);

        var result = workspace.AddWorkItem(workItem);
        return result.IsSuccess ? workItem : throw new InvalidOperationException(result.Error);
    }
}
