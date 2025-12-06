using NodaTime;

namespace Moda.Common.Domain.Events.Organization;

public record TeamDeactivatedEvent : DomainEvent
{
    public TeamDeactivatedEvent(Guid id, LocalDate inactiveDate, Instant timestamp)
    {
        Id = id;
        InactiveDate = inactiveDate;
        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public LocalDate InactiveDate { get; }
}
