namespace Moda.Common.Domain.Interfaces;

public interface IEntity
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void AddDomainEvent(DomainEvent domainEvent);
    void RemoveDomainEvent(DomainEvent domainEvent);
    void ClearDomainEvents();
}

public interface IEntity<TId> : IEntity
{
    TId Id { get; }
}
