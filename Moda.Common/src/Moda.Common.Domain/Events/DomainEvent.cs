using NodaTime;

namespace Moda.Common.Domain.Events;

public abstract record DomainEvent : IEvent
{
    public Instant Timestamp { get; protected set; }
}
