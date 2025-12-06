using NodaTime;

namespace Moda.Common.Domain.Events.Organization;

public sealed record TeamDeletedEvent : DomainEvent
{
    public TeamDeletedEvent(Guid id, Instant timestamp)
    {
        Id = id;
        Timestamp = timestamp;
    }

    public Guid Id { get; }
}
