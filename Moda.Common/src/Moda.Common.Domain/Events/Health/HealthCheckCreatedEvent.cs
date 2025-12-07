using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Common.Domain.Events.Health;

public sealed record HealthCheckCreatedEvent : DomainEvent
{
    public HealthCheckCreatedEvent(Guid id, Guid objectId, SystemContext context, HealthStatus status, Guid reportedById, Instant reportedOn, Instant expiration, string? note, Instant timestamp)
    {
        Id = id;
        ObjectId = objectId;
        Context = context;
        Status = status;
        ReportedById = reportedById;
        ReportedOn = reportedOn;
        Expiration = expiration;
        Note = note;

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public Guid ObjectId { get; }
    public SystemContext Context { get; }
    public HealthStatus Status { get; }
    public Guid ReportedById { get; }
    public Instant ReportedOn { get; }
    public Instant Expiration { get; }
    public string? Note { get; }
}
