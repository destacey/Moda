using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityUpdatedEvent
{
    public static EntityUpdatedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant created) where TEntity : class, IEntity
        => new(entity, created);
}

public record EntityUpdatedEvent<TEntity> : DomainEvent where TEntity : class, IEntity
{
    internal EntityUpdatedEvent(TEntity entity, Instant triggeredOn)
    {
        Entity = entity;
        TriggeredOn = triggeredOn;
    }

    public TEntity Entity { get; }
}
