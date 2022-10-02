using NodaTime;
using NodaTime.Extensions;

namespace Moda.Common.Domain.Events;

public abstract record DomainEvent : IEvent
{
    public Instant TriggeredOn { get; init; } = DateTime.Now.ToInstant();
}
