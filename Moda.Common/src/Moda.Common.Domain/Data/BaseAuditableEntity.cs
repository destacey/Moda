namespace Moda.Common.Domain.Data;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>, ISystemAuditable
{
}

public abstract class BaseAuditableEntity : BaseAuditableEntity<Guid>
{
    protected BaseAuditableEntity()
    {
        Id = Guid.CreateVersion7();
    }
}
