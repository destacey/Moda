using NodaTime;

namespace Wayd.Common.Domain.Events;

public abstract record DomainEvent : IEvent
{
    public Instant Timestamp { get; protected set; }
}
