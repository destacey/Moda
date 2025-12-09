using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Common.Domain.Events.Health;

public sealed record HealthCheckUpdatedEvent : DomainEvent
{
    public HealthCheckUpdatedEvent(Guid id, Guid objectId, SystemContext context, HealthStatus status, Instant expiration, string? note, Instant timestamp)
    {
        Id = id;
        ObjectId = objectId;
        Context = context;
        Status = status;
        Expiration = expiration;
        Note = note;
        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public Guid ObjectId { get; private init; }
    public SystemContext Context { get; private init; }
    public HealthStatus Status { get; }
    public Instant Expiration { get; }
    public string? Note { get; }
}
