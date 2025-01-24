using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Interfaces;
using NodaTime;

namespace Moda.Common.Models;

/// <summary>
/// A date range that uses <see cref="LocalDate" />.
/// </summary>
/// <seealso cref="CSharpFunctionalExtensions.ValueObject" />
/// <seealso cref="Moda.Common.Interfaces.IDateRange&lt;NodaTime.LocalDate&gt;" />
public class LocalDateRange : ValueObject, IDateRange<LocalDate>
{
    public LocalDateRange(LocalDate start, LocalDate end)
    {
        Start = Guard.Against.Null(start);
        End = Guard.Against.Null(end);

        if (End < Start)
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
    public LocalDate End { get; private set; }

    /// <summary>
    /// Gets the number of days in the range.
    /// </summary>
    public int Days => Period.DaysBetween(Start, End) + 1;

    /// <summary>
    /// Determines whether the range includes the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Includes(LocalDate value)
    {
        return (Start <= value) && (value <= End);
    }

    /// <summary>
    /// Determines whether the range includes the specified range.
    /// </summary>
    /// <param name="otherRange"></param>
    /// <returns></returns>
    public bool Includes(LocalDateRange otherRange)
    {
        return (Start <= otherRange.Start) && (otherRange.End <= End);
    }

    /// <summary>Overlapses the specified range.</summary>
    /// <param name="range">The range.</param>
    /// <returns></returns>
    public bool Overlaps(LocalDateRange range)
    {
        return Includes(range)
            || range.Includes(this)
            || (range.Start <= Start && Start <= range.End && range.End <= End)
            || (Start <= range.Start && range.Start <= End && End <= range.End);
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
        return End < date;
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
        yield return End;
    }
}
