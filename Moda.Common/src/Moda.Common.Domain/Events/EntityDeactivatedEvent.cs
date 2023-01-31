using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityDeactivatedEvent
{
    public static EntityDeactivatedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant timestamp) where TEntity : class, IEntity
        => new(entity, timestamp);
}

public record EntityDeactivatedEvent<TEntity> : DomainEvent, IGenericDomainEvent
    where TEntity : class, IEntity
{
    internal EntityDeactivatedEvent(TEntity entity, Instant deactivatedOn)
    {
        Entity = entity;
        Timestamp = deactivatedOn;
    }

    public TEntity Entity { get; }
}

