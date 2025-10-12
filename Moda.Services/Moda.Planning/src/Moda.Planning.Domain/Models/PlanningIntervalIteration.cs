using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Domain.Interfaces;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Interfaces;
using NodaTime;

namespace Moda.Planning.Domain.Models;
public sealed class PlanningIntervalIteration : BaseSoftDeletableEntity<Guid>, ILocalSchedule, IHasIdAndKey
{
    private string _name = default!;
    private LocalDateRange _dateRange = default!;

    private PlanningIntervalIteration() { }

    internal PlanningIntervalIteration(Guid planningIntervalId, string name, IterationCategory category, LocalDateRange dateRange)
    {
        // TODO guard against default planningIntervalId, but PI would need to generate a new Guid, rather than depend on the DB for this to work.
        PlanningIntervalId = planningIntervalId;
        Name = name;
        Category = category;
        DateRange = dateRange;
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; private init; }

    /// <summary>Gets the planning interval identifier.</summary>
    /// <value>The planning interval identifier.</value>
    public Guid PlanningIntervalId { get; private init; }

    /// <summary>
    /// The name of the Planning Interval.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets or sets the category.</summary>
    /// <value>The iteration category.</value>
    public IterationCategory Category { get; private set; }

    /// <summary>Gets or sets the date range.</summary>
    /// <value>The date range.</value>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        private set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    /// <summary>Iteration state on given date.</summary>
    /// <param name="date">The date.</param>
    /// <returns></returns>
    public IterationState StateOn(LocalDate date)
    {
        if (DateRange.IsPastOn(date)) { return IterationState.Completed; }
        if (DateRange.IsActiveOn(date)) { return IterationState.Active; }
        return IterationState.Future;
    }

    internal Result Update(string name, IterationCategory category, LocalDateRange dateRange)
    {
        Name = name;
        Category = category;
        DateRange = dateRange;

        return Result.Success();
    }

    internal static PlanningIntervalIteration Create(Guid planningIntervalId, string name, IterationCategory category, LocalDateRange dateRange)
    {
        return new PlanningIntervalIteration(planningIntervalId, name, category, dateRange);
    }
}
