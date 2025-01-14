using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;
using Moda.Common.Domain.Interfaces;
using Moda.StrategicManagement.Domain.Enums;
using NodaTime;

namespace Moda.StrategicManagement.Domain.Models;

/// <summary>
/// Represents the overarching purpose and direction of the organization.
/// Operates independently of the Strategy entity, allowing flexibility for different organizational use cases.
/// </summary>
public sealed class Vision : BaseAuditableEntity<Guid>, HasIdAndKey
{
    private string _description = default!;
    private LocalDate? _end;

    private Vision() { }

    private Vision(string description, VisionState state, LocalDate? start, LocalDate? end)
    {
        Description = description;
        State = state;
        Start = start;
        End = end;
    }

    /// <summary>
    /// The unique key of the StrategicTheme.  This is an alternate key to the Id.
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
    /// The date when the vision became active or started guiding the organization.
    /// </summary>
    public LocalDate? Start { get; private set; }

    /// <summary>
    /// The date when the vision was archived or replaced.
    /// </summary>
    public LocalDate? End
    {
        get => _end;
        private set
        {
            if (value.HasValue)
            {
                Guard.Against.Null(Start, nameof(Start), "Start date must be set before setting the end date.");
                Guard.Against.OutOfRange(value.Value, nameof(End), Start.Value.PlusDays(1), LocalDate.MaxIsoValue, "End date must be greater than the start date.");
            }
            _end = value;
        }
    }

    /// <summary>
    /// Updates the Vision.
    /// </summary>
    /// <param name="description"></param>
    /// <param name="state"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Result Update(string description, VisionState state, LocalDate? start, LocalDate? end)
    {
        Description = description;
        State = state;
        Start = start;
        End = end;

        return Result.Success();
    }

    /// <summary>
    /// Creates a new Vision.
    /// </summary>
    /// <param name="description"></param>
    /// <param name="state"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static Vision Create(string description, VisionState state, LocalDate? start, LocalDate? end)
    {
        return new Vision(description, state, start, end);
    }
}
