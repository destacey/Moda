using NodaTime;

namespace Moda.Domain.Common.Data;

public abstract class BaseAuditableEntity : BaseEntity
{
    public Instant Created { get; set; }

    public string? CreatedBy { get; set; }

    public Instant LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
