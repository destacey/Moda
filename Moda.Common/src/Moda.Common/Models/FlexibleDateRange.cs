using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Interfaces;
using NodaTime;

namespace Moda.Common.Models;

/// <summary>
/// A date range that uses <see cref="LocalDate" for the start date and <see cref="LocalDate?"/> for the end date.
/// </summary>
public class FlexibleDateRange : ValueObject, IDateRange<LocalDate, LocalDate?>
{
    public FlexibleDateRange(LocalDate start, LocalDate? end = null)
    {
        Start = Guard.Against.Null(start);
        End = end;

        if (End.HasValue && End < Start)
        {
            throw new ArgumentException("The start date must be on or before the end date.", nameof(LocalDateRange));
        }
    }

    /// <summary>
    /// Gets the start date.
    /// </summary>
    public LocalDate Start { get; private set; }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public LocalDate? End { get; private set; }

    /// <summary>
    /// Gets the effective end date with a default value of <see cref="LocalDate.MaxIsoValue"/>.
    /// </summary>
    public LocalDate EffectiveEnd => End ?? LocalDate.MaxIsoValue;

    /// <summary>
    /// Gets the number of days in the range.
    /// </summary>
    public int Days => Period.DaysBetween(Start, EffectiveEnd) + 1;

    /// <summary>
    /// Determines whether the range includes the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Includes(LocalDate value)
    {
        return Start <= value && value <= EffectiveEnd;
    }

    /// <summary>
    /// Determines whether the range includes the specified range.
    /// </summary>
    /// <param name="otherRange"></param>
    /// <returns></returns>
    public bool Includes(LocalDateRange otherRange)
    {
        return (Start <= otherRange.Start) && (otherRange.End <= EffectiveEnd);
    }

    /// <summary>
    /// Determines whether the range includes the specified range.
    /// </summary>
    /// <param name="otherRange"></param>
    /// <returns></returns>
    public bool Includes(FlexibleDateRange otherRange)
    {
        return Includes(otherRange.ToLocalDateRange());
    }

    /// <summary>
    /// Determines whether the range overlaps the specified range.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool Overlaps(LocalDateRange range)
    {
        return Includes(range)
            || range.Includes(this.ToLocalDateRange())
            || (range.Start <= Start && Start <= range.End && range.End <= EffectiveEnd)
            || (Start <= range.Start && range.Start <= EffectiveEnd && EffectiveEnd <= range.End);
    }

    /// <summary>
    /// Determines whether [is past on] [the specified date].
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>
    ///   <c>true</c> if [is past on] [the specified date]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsPastOn(LocalDate date)
    {
        return EffectiveEnd < date;
    }

    /// <summary>
    /// Determines whether [is active on] [the specified date].
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>
    ///   <c>true</c> if [is active on] [the specified date]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsActiveOn(LocalDate date)
    {
        return Includes(date);
    }

    /// <summary>
    /// Determines whether [is future on] [the specified date].
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>
    ///   <c>true</c> if [is future on] [the specified date]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsFutureOn(LocalDate date)
    {
        return date < Start;
    }

    /// <summary>
    /// Gets the equality components.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Start;
        yield return EffectiveEnd;
    }

    /// <summary>
    /// Converts the <see cref="FlexibleDateRange"/> to a <see cref="LocalDateRange"/>.
    /// </summary>
    /// <returns></returns>
    public LocalDateRange ToLocalDateRange()
    {
        return new LocalDateRange(Start, EffectiveEnd);
    }
}
