using NodaTime;

namespace Moda.Common.Domain.Data;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>, IAuditable
{
    public Instant Created { get; set; }

    public string? CreatedBy { get; set; }

    public Instant LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
