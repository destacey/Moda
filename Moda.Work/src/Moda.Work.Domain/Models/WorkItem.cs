namespace Moda.Work.Domain.Models;

public sealed class WorkItem : BaseAuditableEntity<Guid>
{
    private readonly List<WorkItemRevision> _history = new();

    private WorkItem() { }

    /// <summary>
    /// A URL and display friendly unique Id.
    /// </summary>
    public int Number { get; set; }

    public string Title { get; set; } = null!;

    public Guid WorkspaceId { get; private set; }

    public Workspace Workspace { get; private set; } = null!;

    public Guid TypeId { get; private set; }

    public WorkType Type { get; private set; } = null!;

    public Guid StateId { get; private set; }

    public WorkState State { get; private set; } = null!;

    public Guid BacklogLevelId { get; private set; }

    public BacklogLevel BacklogLevel { get; private set; } = null!;

    public Guid WorkProcessConfigurationId { get; private set; }

    public WorkProcessScheme WorkProcessConfiguration { get; private set; } = null!;

    /// <summary>
    /// The reason the work item is in its current state.
    /// </summary>
    public string? StateReason { get; set; }

    public int Priority { get; set; }

    public string? ValueArea { get; set; }


    /// <summary>
    /// The collection of revisions for the life of the work item.
    /// </summary>
    public IReadOnlyCollection<WorkItemRevision> History => _history.AsReadOnly();
}
