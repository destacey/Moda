using NodaTime;

namespace Moda.Common.Domain.Events;

public static class EntityDeletedEvent
{
    public static EntityDeletedEvent<TEntity, TId> WithEntity<TEntity, TId>(TEntity entity, Instant created) where TEntity : class, IEntity<TId>
        => new(entity, created);
}

public record EntityDeletedEvent<TEntity, TId> : DomainEvent where TEntity : class, IEntity<TId>
{
    internal EntityDeletedEvent(TEntity entity, Instant created)
    {
        Entity = entity;
        Created = created;
    }

    public TEntity Entity { get; }
}
