using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Interfaces;
using NodaTime;

namespace Moda.Organization.Domain.Models;

/// <summary>
/// Represents the effective date range for a team operating model.
/// </summary>
public class OperatingModelDateRange : ValueObject, IDateRange<LocalDate, LocalDate?>
{
    public OperatingModelDateRange(LocalDate start, LocalDate? end)
    {
        Start = Guard.Against.Null(start);
        End = end;

        if (End.HasValue && End < Start)
        {
            throw new ArgumentException("The start date must be on or before the end date.", nameof(start));
        }
    }

    /// <summary>Gets the start date of the operating model.</summary>
    public LocalDate Start { get; private set; }

    /// <summary>Gets the end date of the operating model. Null indicates the model is current.</summary>
    public LocalDate? End { get; private set; }

    /// <summary>Gets whether this operating model is current (has no end date).</summary>
    public bool IsCurrent => End is null;

    /// <summary>Determines if the specified date falls within this date range.</summary>
    /// <param name="value">The date to check.</param>
    /// <returns>True if the date is within the range, otherwise false.</returns>
    public bool Includes(LocalDate value)
    {
        return End is null
            ? Start <= value
            : (Start <= value) && (value <= End);
    }

    /// <summary>Determines if this date range overlaps with another.</summary>
    /// <param name="range">The other date range to check.</param>
    /// <returns>True if the ranges overlap, otherwise false.</returns>
    public bool Overlaps(OperatingModelDateRange range)
    {
        if (End is null && range.End is null)
            return true;

        if (End is null)
            return Start <= range.End;

        if (range.End is null)
            return range.Start <= End;

        return Start <= range.End && range.Start <= End;
    }

    /// <summary>Sets the end date for this operating model. Used when closing a model.</summary>
    /// <param name="endDate">The end date to set.</param>
    internal void SetEnd(LocalDate endDate)
    {
        if (endDate < Start)
            throw new ArgumentException("The end date cannot be before the start date.", nameof(endDate));

        End = endDate;
    }

    internal void ClearEnd()
    {
        End = null;
    }

    protected override IEnumerable<IComparable?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
