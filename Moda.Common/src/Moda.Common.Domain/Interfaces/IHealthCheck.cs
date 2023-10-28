using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Common.Domain.Interfaces;
public interface IHealthCheck
{
    int Id { get; }
    Guid ObjectId { get; }
    HealthCheckContext Context { get; }
    Instant Expiration { get; }
    string? Note { get; }
}
