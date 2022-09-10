using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityUpdatedEvent
{
    public static EntityUpdatedEvent<TEntity, TId> WithEntity<TEntity, TId>(TEntity entity, Instant created) where TEntity : class, IEntity<TId>
        => new(entity, created);
}

public record EntityUpdatedEvent<TEntity, TId> : DomainEvent where TEntity : class, IEntity<TId>
{
    internal EntityUpdatedEvent(TEntity entity, Instant created)
    {
        Entity = entity;
        Created = created;
    }

    public TEntity Entity { get; }
}
