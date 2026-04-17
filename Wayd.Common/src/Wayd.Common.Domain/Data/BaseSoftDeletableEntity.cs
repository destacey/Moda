using NodaTime;

namespace Wayd.Common.Domain.Data;

public abstract class BaseSoftDeletableEntity<TId> : BaseAuditableEntity<TId>, ISoftDelete
{
    /// <summary>
    /// The date and time the record was deleted.
    /// </summary>
    public Instant? Deleted { get; set; }

    /// <summary>
    /// The employee that deleted this record.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Flag to determine if the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

public abstract class BaseSoftDeletableEntity : BaseSoftDeletableEntity<Guid>
{
    protected BaseSoftDeletableEntity()
    {
        Id = Guid.CreateVersion7();
    }
}
