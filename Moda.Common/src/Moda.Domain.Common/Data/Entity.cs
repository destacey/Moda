﻿using System.ComponentModel.DataAnnotations.Schema;
using Moda.Common.Domain.Events;

namespace Moda.Common.Domain.Data;

public abstract class Entity<TId> : IEntity<TId>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    public TId Id { get; set; } = default!;
    

    private readonly List<DomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}