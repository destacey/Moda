namespace Moda.Common.Domain.Data;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>, ISystemAuditable
{
}
