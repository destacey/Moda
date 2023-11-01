using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Common.Domain.Interfaces;
public interface IHealthCheck
{
    Guid Id { get; }
    Guid ObjectId { get; }
    HealthCheckContext Context { get; }
    HealthStatus Status { get; }
    Guid ReportedById { get; }
    Instant ReportedOn { get; }
    Instant Expiration { get; }
    string? Note { get; }

    bool IsExpired(Instant now);
}
