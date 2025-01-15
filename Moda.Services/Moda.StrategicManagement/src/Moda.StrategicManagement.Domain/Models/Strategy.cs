using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.StrategicManagement.Domain.Enums;
using NodaTime;

namespace Moda.StrategicManagement.Domain.Models;

/// <summary>
/// Represents a strategic plan or initiative to achieve organizational goals.
/// </summary>
public class Strategy : BaseEntity<Guid>, ISystemAuditable, HasIdAndKey
{
    private string _name = default!;
    private string? _description = default!;
    private LocalDate? _end;

    private Strategy() { }

    private Strategy(string name, string? description, StrategyStatus status, LocalDate? start, LocalDate? end)
    {
        Name = name;
        Description = description;
        Status = status;
        Start = start;
        End = end;
    }

    /// <summary>
    /// The unique key of the strategy.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The concise statement describing the strategy and its purpose or focus area.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// A detailed description of the strategy.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// The current status of the strategy (e.g., Draft, Active, Completed, Archived).
    /// </summary>
    public StrategyStatus Status { get; private set; }

    /// <summary>
    /// The date when the strategy was initiated or became active.
    /// </summary>
    public LocalDate? Start { get; private set; }

    /// <summary>
    /// The date when the strategy was completed, archived, or ended.
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
    /// Updates the Strategy.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="status"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Result Update(string name, string description, StrategyStatus status, LocalDate? start, LocalDate? end)
    {
        Name = name;
        Description = description;
        Status = status;
        Start = start;
        End = end;

        return Result.Success();
    }

    /// <summary>
    /// Creates a new Strategy.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="status"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static Strategy Create(string name, string? description, StrategyStatus status, LocalDate? start, LocalDate? end)
    {
        return new Strategy(name, description, status, start, end);
    }
}

