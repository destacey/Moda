using NodaTime;

namespace Moda.Common.Domain.Data;

public abstract class BaseAuditableEntity<TId> : Entity<TId>, IAudited, IDeletionAudited
{
    public Instant Created { get; set; }

    public string? CreatedBy { get; set; }

    public Instant LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

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
