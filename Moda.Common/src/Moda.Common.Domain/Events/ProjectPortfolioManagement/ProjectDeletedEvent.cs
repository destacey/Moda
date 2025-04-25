using NodaTime;

namespace Moda.Common.Domain.Events.ProjectPortfolioManagement;

public record ProjectDeletedEvent : DomainEvent
{
    public ProjectDeletedEvent(Guid id, Instant timestamp)
    {
        Id = id;

        Timestamp = timestamp;
    }

    public Guid Id { get; init; }
}
