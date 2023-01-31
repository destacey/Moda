using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityCreatedEvent
{
    public static EntityCreatedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant timestamp) where TEntity : class, IEntity
        => new(entity, timestamp);
}

public record EntityCreatedEvent<TEntity> : DomainEvent, IGenericDomainEvent 
    where TEntity : class, IEntity
{
    internal EntityCreatedEvent(TEntity entity, Instant timestamp)
    {
        Entity = entity;
        Timestamp = timestamp;
    }

    public TEntity Entity { get; }
}
