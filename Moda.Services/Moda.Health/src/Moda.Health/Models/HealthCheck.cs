using Moda.Common.Domain.Data;
using Moda.Common.Domain.Enums;
using Moda.Common.Extensions;
using Moda.Common.Domain.Interfaces;
using NodaTime;
using CSharpFunctionalExtensions;
using Ardalis.GuardClauses;

namespace Moda.Health.Models;

public sealed class HealthCheck : BaseAuditableEntity<int>, IHealthCheck
{
    private string? _note;
    private Instant _expiration;

    private HealthCheck() { }

    internal HealthCheck(Guid objectId, HealthCheckContext context, HealthStatus status, Instant timestamp, Instant expiration, string? note)
    {
        Guard.Against.Default(objectId, nameof(objectId));

        ObjectId = objectId;
        Context = context;
        Status = status;
        Timestamp = timestamp;
        Expiration = expiration;
        Note = note;
    }


    /// <summary>
    /// The objectId associated with the health check.
    /// </summary>
    public Guid ObjectId { get; private init; }

    /// <summary>
    /// The context of the health check based on the ObjectId.
    /// </summary>
    public HealthCheckContext Context { get; private init; }

    /// <summary>
    /// The status of the health check.
    /// </summary>
    public HealthStatus Status { get; private set; }

    /// <summary>
    /// The timestamp of when the health check was initially created.
    /// </summary>
    public Instant Timestamp { get; private init; }

    /// <summary>
    /// The expiration of the health check.
    /// </summary>
    public Instant Expiration
    {
        get => _expiration;
        private set
        {
            if (value <= Timestamp)
            {
                throw new ArgumentException("Expiration must be greater than timestamp.", nameof(Expiration));
            }

            _expiration = value;
        }
    }

    /// <summary>
    /// The note for the health check.
    /// </summary>
    public string? Note
    {
        get => _note;
        private set => _note = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// Is the health check expired.
    /// </summary>
    public bool IsExpired(Instant now) => Expiration <= now;

    /// <summary>
    /// Change the expiration of the health check.
    /// </summary>
    /// <param name="expiration"></param>
    internal void ChangeExpiration(Instant expiration)
    {
        Expiration = expiration;
    }

    /// <summary>
    /// Update method for health check.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="expiration"></param>
    /// <param name="note"></param>
    /// <returns>Result</returns>
    internal Result Update(HealthStatus status, Instant expiration, string? note)
    {
        try
        {
            Status = status;
            Expiration = expiration;
            Note = note;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message.ToString());
        }
    }
}
