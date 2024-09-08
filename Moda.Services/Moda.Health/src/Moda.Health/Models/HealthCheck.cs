using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;
using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events;
using Moda.Common.Domain.Interfaces;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Health.Models;

public sealed class HealthCheck : BaseSoftDeletableEntity<Guid>, IHealthCheck
{
    private string? _note;
    private Instant _expiration;

    private HealthCheck() { }

    internal HealthCheck(Guid objectId, SystemContext context, HealthStatus status, Guid reportedById, Instant reportedOn, Instant expiration, string? note)
    {
        Guard.Against.Default(objectId, nameof(objectId));

        ObjectId = objectId;
        Context = context;
        Status = status;
        ReportedById = reportedById;
        ReportedOn = reportedOn;
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
    public SystemContext Context { get; private init; }

    /// <summary>
    /// The status of the health check.
    /// </summary>
    public HealthStatus Status { get; private set; }

    /// <summary>
    /// EmployeeId of the employee who reported the health check.
    /// </summary>
    public Guid ReportedById { get; private set; }

    /// <summary>
    /// The employee who reported the health check.
    /// </summary>
    public Employee ReportedBy { get; private set; } = default!;

    /// <summary>
    /// The timestamp of when the health check was initially created.
    /// </summary>
    public Instant ReportedOn { get; private init; }

    /// <summary>
    /// The expiration of the health check.
    /// </summary>
    public Instant Expiration
    {
        get => _expiration;
        private set
        {
            if (value <= ReportedOn)
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
    internal Result Update(HealthStatus status, Instant expiration, string? note, Instant now)
    {
        if (IsExpired(now))
            return Result.Failure("Expired health checks cannot be modified.");

        try
        {
            Status = status;
            Expiration = expiration;
            Note = note;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, now));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message.ToString());
        }
    }
}
