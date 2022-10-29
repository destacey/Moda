using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityDeletedEvent
{
    public static EntityDeletedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant created) where TEntity : class, IEntity
        => new(entity, created);
}

public record EntityDeletedEvent<TEntity> : DomainEvent, IGenericDomainEvent
    where TEntity : class, IEntity
{
    internal EntityDeletedEvent(TEntity entity, Instant triggeredOn)
    {
        Entity = entity;
        TriggeredOn = triggeredOn;
    }

    public TEntity Entity { get; }
}
