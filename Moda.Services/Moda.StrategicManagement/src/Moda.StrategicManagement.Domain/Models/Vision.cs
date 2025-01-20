using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.StrategicManagement.Domain.Enums;
using NodaTime;

namespace Moda.StrategicManagement.Domain.Models;

/// <summary>
/// Represents the overarching purpose and direction of the organization.
/// Operates independently of the Strategy entity, allowing flexibility for different organizational use cases.
/// </summary>
public sealed class Vision : BaseEntity<Guid>, ISystemAuditable, HasIdAndKey
{
    private string _description = default!;

    private Vision() { }

    private Vision(string description)
    {
        Description = description;
        State = VisionState.Proposed;
    }

    /// <summary>
    /// The unique key of the vision.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// A concise statement describing the vision of the organization.
    /// </summary>
    public string Description
    {
        get => _description;
        private set => _description = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    }

    /// <summary>
    /// The current lifecycle state of the vision (e.g., Active, Proposed, Archived).
    /// </summary>
    public VisionState State { get; private set; }

    /// <summary>
    /// The start and end dates of when the vision was active.
    /// </summary>
    public FlexibleInstantRange? Dates { get; private set; }

    /// <summary>
    /// Updates the Vision.
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public Result Update(string description)
    {
        if (State == VisionState.Archived)
        {
            return Result.Failure("The vision is archived and cannot be updated.");
        }

        Description = description;

        return Result.Success();
    }

    internal Result Activate(Instant startDate)
    {
        if (State != VisionState.Proposed)
        {
            return Result.Failure("The vision must be in the Proposed state to activate it.");
        }

        State = VisionState.Active;
        Dates = new FlexibleInstantRange(startDate);

        return Result.Success();
    }

    internal Result Archive(Instant endDate)
    {
        var canArchiveResult = CanArchive(endDate);
        if (canArchiveResult.IsFailure)
        {
            return canArchiveResult;
        }

        State = VisionState.Archived;
        Dates = new FlexibleInstantRange(Dates!.Start, endDate);

        return Result.Success();
    }

    internal Result CanArchive(Instant endDate)
    {
        if (State != VisionState.Active)
        {
            return Result.Failure("The vision must be in the Active state to archive it.");
        }
        if (endDate < Dates!.Start)
        {
            return Result.Failure("The end date must be on or after the start date.");
        }

        return Result.Success();
    }

    /// <summary>
    /// Creates a new Vision.
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static Vision Create(string description)
    {
        return new Vision(description);
    }
}
