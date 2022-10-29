using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityActivatedEvent
{
    public static EntityActivatedEvent<TEntity> WithEntity<TEntity>(TEntity entity, Instant activatedOn) where TEntity : class, IEntity
        => new(entity, activatedOn);
}

public record EntityActivatedEvent<TEntity> : DomainEvent, IGenericDomainEvent
    where TEntity : class, IEntity
{
    internal EntityActivatedEvent(TEntity entity, Instant activatedOn)
    {
        Entity = entity;
        TriggeredOn = activatedOn;
    }

    public TEntity Entity { get; }
}

