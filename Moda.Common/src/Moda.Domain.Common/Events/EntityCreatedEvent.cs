using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityCreatedEvent
{
    public static EntityCreatedEvent<TEntity, TId> WithEntity<TEntity, TId>(TEntity entity, Instant created) where TEntity : class, IEntity<TId>
        => new(entity, created);
}

public record EntityCreatedEvent<TEntity, TId> : DomainEvent where TEntity : class, IEntity<TId>
{
    internal EntityCreatedEvent(TEntity entity, Instant created)
    {
        Entity = entity;
        Created = created;
    }

    public TEntity Entity { get; }
}
