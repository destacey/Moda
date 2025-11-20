using NodaTime;

namespace Moda.Common.Domain.Events.ProjectPortfolioManagement;

public record ProgramDeletedEvent : DomainEvent
{
    public ProgramDeletedEvent(Guid id, Instant timestamp)
    {
        Id = id;

        Timestamp = timestamp;
    }

    public Guid Id { get; init; }
}
