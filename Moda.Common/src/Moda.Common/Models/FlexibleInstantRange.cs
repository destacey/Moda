using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Interfaces;
using NodaTime;

namespace Moda.Common.Models;

/// <summary>
/// A date range that uses <see cref="Instant" /> for the start and a nullable <see cref="Instant" /> for the end.
/// </summary>
public class FlexibleInstantRange : ValueObject, IDateRange<Instant, Instant?>
{
    public FlexibleInstantRange(Instant start, Instant? end = null)
    {
        Start = Guard.Against.Null(start);
        End = end;

        if (End.HasValue && End < Start)
        {
            throw new ArgumentException("The start date must be on or before the end date.", nameof(FlexibleInstantRange));
        }
    }

    /// <summary>
    /// Gets the start date.
    /// </summary>
    public Instant Start { get; private set; }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public Instant? End { get; private set; }

    /// <summary>
    /// Gets the effective end date with a default value of <see cref="Instant.MaxValue"/>.
    /// </summary>
    public Instant EffectiveEnd => End ?? Instant.MaxValue;

    /// <summary>
    /// Gets the number of days in the range.
    /// </summary>
    public int Days => (EffectiveEnd - Start).Days + 1;

    /// <summary>
    /// Determines whether the range includes the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Includes(Instant value)
    {
        return Start <= value && value <= EffectiveEnd;
    }

    /// <summary>
    /// Determines whether the range includes the specified range.
    /// </summary>
    /// <param name="otherRange"></param>
    /// <returns></returns>
    public bool Includes(FlexibleInstantRange otherRange)
    {
        return (Start <= otherRange.Start) && (otherRange.EffectiveEnd <= EffectiveEnd);
    }

    /// <summary>
    /// Determines whether the range overlaps the specified range.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool Overlaps(FlexibleInstantRange range)
    {
        return Includes(range)
            || range.Includes(this)
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
    public bool IsPastOn(Instant date)
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
    public bool IsActiveOn(Instant date)
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
    public bool IsFutureOn(Instant date)
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
}
