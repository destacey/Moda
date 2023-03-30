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

    /// <summary>Gets the start.</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; private set; }

    /// <summary>Gets the end.</summary>
    /// <value>The end.</value>
    public LocalDate End { get; private set; }

    /// <summary>Gets the days.</summary>
    /// <value>The days.</value>
    public int Days => Period.DaysBetween(Start, End) + 1;

    /// <summary>Includeses the specified value.</summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool Includes(LocalDate value)
    {
        return (Start <= value) && (value <= End);
    }

    /// <summary>Includes the specified range.</summary>
    /// <param name="range">The range.</param>
    /// <returns></returns>
    public bool Includes(LocalDateRange range)
    {
        return (Start <= range.Start) && (range.End <= End);
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

    /// <summary>Gets the equality components.</summary>
    /// <returns></returns>
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
