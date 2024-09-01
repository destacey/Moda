using NodaTime;

namespace Moda.Common.Domain.Data;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>, IAuditable
{
    public Instant Created { get; set; }

    public Guid? CreatedBy { get; set; }

    public Instant LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
