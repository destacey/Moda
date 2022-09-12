namespace Moda.Work.Domain.Models;

public class WorkItem : BaseAuditableEntity<Guid>
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

    public Guid WorkTypeId { get; private set; }

    public WorkType WorkType { get; private set; } = null!;

    public Guid WorkStatusId { get; private set; }

    public WorkStatus WorkStatus { get; private set; } = null!;

    public string? StatusReason { get; set; }

    public int Priority { get; set; }

    public string? ValueArea { get; set; }


    /// <summary>
    /// The collection of revisions for the life of the work item.
    /// </summary>
    public IReadOnlyCollection<WorkItemRevision> History => _history.AsReadOnly();
}
