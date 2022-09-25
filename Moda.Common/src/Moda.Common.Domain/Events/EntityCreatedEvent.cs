using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityCreatedEvent
{
    public static EntityCreatedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant created) where TEntity : class, IEntity
        => new(entity, created);
}

public record EntityCreatedEvent<TEntity> : DomainEvent where TEntity : class, IEntity
{
    internal EntityCreatedEvent(TEntity entity, Instant triggeredOn)
    {
        Entity = entity;
        TriggeredOn = triggeredOn;
    }

    public TEntity Entity { get; }
}
