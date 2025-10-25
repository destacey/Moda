using NodaTime;

namespace Moda.Common.Domain.Events.Planning.Iterations;
public sealed record IterationDeletedEvent : DomainEvent
{
    public IterationDeletedEvent(Guid id, Instant timestamp)
    {
        Id = id;

        Timestamp = timestamp;
    }

    public Guid Id { get; }
}
