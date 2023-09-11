using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A specific field change within a work item revision.
/// </summary>
public sealed class WorkItemRevisionChange : BaseEntity<Guid>, ISoftDelete
{
    private WorkItemRevisionChange() { }

    public WorkItemRevisionChange(Guid workItemRevisionId, string fieldName, string? oldValue, string? newValue)
    {
        WorkItemRevisionId = workItemRevisionId;
        FieldName = fieldName.Trim();
        OldValue = oldValue?.Trim();
        NewValue = newValue?.Trim();
    }

    /// <summary>
    /// The foreign key for the work item revision this change is for.
    /// </summary>
    public Guid WorkItemRevisionId { get; }

    /// <summary>
    /// The name of the field that was changed
    /// </summary>
    public string FieldName { get; } = null!;

    // TODO this is only handling strings.  How are storing values for dates, numbers, etc???

    /// <summary>
    /// The previous value for the field.
    /// </summary>
    public string? OldValue { get; }

    /// <summary>
    /// The new value for the field.
    /// </summary>
    public string? NewValue { get; }

    /// <summary>
    /// The date and time the record was deleted.
    /// </summary>
    public Instant? Deleted { get; set; }

    /// <summary>
    /// The employee that deleted this record.
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Flag to determine if the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
