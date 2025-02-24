using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Domain.Models;

/// <summary>
/// Represents a strategic plan or initiative to achieve organizational goals.
/// </summary>
public sealed class Strategy : BaseEntity<Guid>, ISystemAuditable, HasIdAndKey
{
    private string _name = default!;
    private string? _description;

    private Strategy() { }

    private Strategy(string name, string? description, StrategyStatus status, FlexibleDateRange? dates)
    {
        Name = name;
        Description = description;
        Status = status;
        Dates = dates;
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

    // TODO: lock these dates down based on the status.  Active should have a start date, Completed should have an end date, etc.

    /// <summary>
    /// The start and end dates of when the strategy was active.
    /// </summary>
    public FlexibleDateRange? Dates { get; private set; }

    /// <summary>
    /// Updates the Strategy.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="status"></param>
    /// <param name="dates"></param>
    /// <returns></returns>
    public Result Update(string name, string description, StrategyStatus status, FlexibleDateRange? dates)
    {
        Name = name;
        Description = description;
        Status = status;
        Dates = dates;

        return Result.Success();
    }

    /// <summary>
    /// Creates a new Strategy.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="status"></param>
    /// <param name="dates"></param>
    /// <returns></returns>
    public static Strategy Create(string name, string? description, StrategyStatus status, FlexibleDateRange? dates)
    {
        return new Strategy(name, description, status, dates);
    }
}
