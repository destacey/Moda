using NodaTime;

namespace Moda.Common.Domain.Events.StrategicManagement;
public sealed record StrategicThemeDeletedEvent : DomainEvent
{
    public StrategicThemeDeletedEvent(Guid id, Instant timestamp)
    {
        Id = id;

        Timestamp = timestamp;
    }

    public Guid Id { get; init; }
}
