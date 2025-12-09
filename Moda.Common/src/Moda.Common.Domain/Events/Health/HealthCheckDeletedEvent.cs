using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Common.Domain.Events.Health;

public sealed record HealthCheckDeletedEvent : DomainEvent
{
    public HealthCheckDeletedEvent(Guid id, Guid objectId, SystemContext context, Instant timestamp)
    {
        Id = id;
        ObjectId = objectId;
        Context = context;

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public Guid ObjectId { get; private init; }
    public SystemContext Context { get; private init; }
}
