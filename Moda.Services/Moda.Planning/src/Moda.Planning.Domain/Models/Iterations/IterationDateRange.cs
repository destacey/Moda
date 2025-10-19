using CSharpFunctionalExtensions;
using Moda.Common.Interfaces;
using NodaTime;

namespace Moda.Planning.Domain.Models.Iterations;
public class IterationDateRange : ValueObject, IDateRange<Instant?>
{
    public IterationDateRange(Instant? start, Instant? end)
    {
        Start = start;
        End = end;

        if (Start.HasValue && End.HasValue && End < Start)
        {
            throw new ArgumentException("The start date must be on or before the end date.", nameof(IterationDateRange));
        }
    }

    /// <summary>
    /// Gets the start date and time, if available.
    /// </summary>
    public Instant? Start { get; private set; }

    /// <summary>
    /// Gets the end date and time, if available.
    /// </summary>
    public Instant? End { get; private set; }

    /// <summary>
    /// Gets the effective start date and time. If <see cref="Start"/> is null, this will return <see cref="Instant.MinValue"/>.
    /// </summary>
    public Instant EffectiveStart => Start ?? Instant.MinValue;

    /// <summary>
    /// Gets the effective end date and time. If <see cref="Start"/> is null, this will return <see cref="Instant.MaxValue"/>.
    /// </summary>
    public Instant EffectiveEnd => End ?? Instant.MaxValue;

    /// <summary>
    /// Gets the number of days in the range.
    /// </summary>
    public int Days => (EffectiveEnd - EffectiveStart).Days + 1;

    /// <summary>
    /// Determines whether the range includes the specified value.
    /// </summary>
    /// <param name="value">If null, it is treated as <see cref="Instant.MinValue"/>.</param>
    /// <returns></returns>
    public bool Includes(Instant? value)
    {
        value ??= Instant.MinValue;
        return EffectiveStart <= value && value <= EffectiveEnd;
    }

    /// <summary>
    /// Determines whether the range includes the specified range.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool Includes(IterationDateRange range)
    {
        return (EffectiveStart <= range.EffectiveStart) && (range.EffectiveEnd <= EffectiveEnd);
    }

    /// <summary>
    /// Determines whether the range overlaps the specified range.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool Overlaps(IterationDateRange range)
    {
        return Includes(range)
            || range.Includes(this)
            || (range.EffectiveStart <= EffectiveStart && EffectiveStart <= range.EffectiveEnd && range.EffectiveEnd <= EffectiveEnd)
            || (EffectiveStart <= range.EffectiveStart && range.EffectiveStart <= EffectiveEnd && EffectiveEnd <= range.EffectiveEnd);
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
        return date < EffectiveStart;
    }

    /// <summary>
    /// Gets the equality components.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return EffectiveStart;
        yield return EffectiveEnd;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="IterationDateRange"/> class with the specified start and end dates.
    /// </summary>
    /// <param name="start">The optional start date of the iteration. Can be <see langword="null"/> to indicate no start date.</param>
    /// <param name="end">The optional end date of the iteration. Can be <see langword="null"/> to indicate no end date.</param>
    /// <returns>A new <see cref="IterationDateRange"/> instance initialized with the specified start and end dates.</returns>
    public static IterationDateRange Create(Instant? start, Instant? end)
        => new(start, end);
}
