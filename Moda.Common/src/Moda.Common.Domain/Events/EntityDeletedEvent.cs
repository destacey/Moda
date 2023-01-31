using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityDeletedEvent
{
    public static EntityDeletedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant timestamp) where TEntity : class, IEntity
        => new(entity, timestamp);
}

public record EntityDeletedEvent<TEntity> : DomainEvent, IGenericDomainEvent
    where TEntity : class, IEntity
{
    internal EntityDeletedEvent(TEntity entity, Instant timestamp)
    {
        Entity = entity;
        Timestamp = timestamp;
    }

    public TEntity Entity { get; }
}
