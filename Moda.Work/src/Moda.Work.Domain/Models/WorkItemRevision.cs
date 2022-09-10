using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// The work item revision is a change record for a work item.
/// </summary>
public class WorkItemRevision : Entity<Guid>, IDeletionAudited
{
    private readonly List<WorkItemRevisionChange> _changes = new();

    private WorkItemRevision() { }

    public WorkItemRevision(Guid workItemId, int revision, Guid? revisedById, Instant revisedDate, IEnumerable<WorkItemRevisionChange> changes)
    {
        WorkItemId = workItemId;
        Revision = revision;
        RevisedById = revisedById;
        RevisedDate = revisedDate;
        
        if (changes?.Count() > 0)
            _changes.AddRange(changes.ToList());
    }

    /// <summary>
    /// The foreign key for the work item this revision is for.
    /// </summary>
    public Guid WorkItemId { get; }

    public WorkItem WorkItem { get; } = null!;

    /// <summary>
    /// A number that represents which revision for the work item this is.
    /// </summary>
    public int Revision { get; }

    /// <summary>
    /// The foreign key for the employee who made the revision.
    /// </summary>
    public Guid? RevisedById { get; }

    /// <summary>
    /// The date and time of the revision.
    /// </summary>
    public Instant RevisedDate { get; }

    /// <summary>
    /// A list of field changes that occurred in the revision.
    /// </summary>
    public IReadOnlyCollection<WorkItemRevisionChange> Changes => _changes.AsReadOnly();

    /// <summary>
    /// The date and time the record was deleted.
    /// </summary>
    public Instant Deleted { get; set; }

    /// <summary>
    /// The employee that deleted this record.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Flag to determine if the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
