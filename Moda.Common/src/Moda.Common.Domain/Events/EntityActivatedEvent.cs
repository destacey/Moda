using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityActivatedEvent
{
    public static EntityActivatedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant timestamp) where TEntity : class, IEntity
        => new(entity, timestamp);
}

public record EntityActivatedEvent<TEntity> : DomainEvent, IGenericDomainEvent
    where TEntity : class, IEntity
{
    internal EntityActivatedEvent(TEntity entity, Instant timestamp)
    {
        Entity = entity;
        Timestamp = timestamp;
    }

    public TEntity Entity { get; }
}

