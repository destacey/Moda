using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityUpdatedEvent
{
    public static EntityUpdatedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant timestamp) where TEntity : class, IEntity
        => new(entity, timestamp);
}

public record EntityUpdatedEvent<TEntity> : DomainEvent, IGenericDomainEvent
    where TEntity : class, IEntity
{
    internal EntityUpdatedEvent(TEntity entity, Instant timestamp)
    {
        Entity = entity;
        Timestamp = timestamp;
    }

    public TEntity Entity { get; }
}
