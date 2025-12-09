using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Planning.Domain.Models;
public sealed class SimpleHealthCheck : BaseEntity<Guid>
{
    private SimpleHealthCheck() { }

    public SimpleHealthCheck(Guid objectId, Guid id, HealthStatus status, Instant reportedOn, Instant expiration)
    {
        Guard.Against.Default(objectId, nameof(objectId));
        Guard.Against.Default(id, nameof(id));

        ObjectId = objectId;
        Id = id;
        Status = status;
        ReportedOn = reportedOn;
        Expiration = expiration;
    }

    /// <summary>
    /// The objectId associated with the health check.
    /// </summary>
    public Guid ObjectId { get; private init; }

    /// <summary>
    /// The status of the health check.
    /// </summary>
    public HealthStatus Status { get; private set; }

    /// <summary>
    /// The timestamp of when the health check was initially created.
    /// </summary>
    public Instant ReportedOn { get; private init; }

    /// <summary>
    /// The expiration of the health check.
    /// </summary>
    public Instant Expiration { get; private set; }

    /// <summary>
    /// Is the health check expired.
    /// </summary>
    public bool IsExpired(Instant now) => Expiration <= now;

    /// <summary>
    /// Update method for health check.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="expiration"></param>
    /// <returns>Result</returns>
    public Result Update(HealthStatus status, Instant expiration)
    {
        try
        {
            Status = status;
            Expiration = expiration;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message.ToString());
        }
    }
}
