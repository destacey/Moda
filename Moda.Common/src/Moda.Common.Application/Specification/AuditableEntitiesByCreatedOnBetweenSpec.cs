using Ardalis.Specification;
using NodaTime;

namespace Moda.Common.Application.Specification;

public class AuditableEntitiesByCreatedOnBetweenSpec<T,TId> : Specification<T>
    where T : BaseAuditableEntity<TId>
{
    public AuditableEntitiesByCreatedOnBetweenSpec(Instant from, Instant until) =>
        Query.Where(e => e.Created >= from && e.Created <= until);
}