﻿using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Interfaces;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public class MembershipDateRange : ValueObject, IDateRange<LocalDate, LocalDate?>
{
    public MembershipDateRange(LocalDate start, LocalDate? end)
    {
        Start = Guard.Against.Null(start);
        End = end;

        if (End.HasValue && End < Start)
        {
            throw new ArgumentException("The start date must be on or before the end date.", nameof(LocalDateRange));
        }
    }

    /// <summary>Gets the start.</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; private set; }

    /// <summary>Gets the end.</summary>
    /// <value>The end.</value>
    public LocalDate? End { get; private set; }

    /// <summary>Gets the days.</summary>
    /// <value>The days.</value>
    //public int Days => Period.DaysBetween(Start, End) + 1;

    /// <summary>Includeses the specified value.</summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool Includes(LocalDate value)
    {
        return End is null
            ? Start <= value
            : (Start <= value) && (value <= End);
    }

    /// <summary>Includes the specified range.</summary>
    /// <param name="range">The range.</param>
    /// <returns></returns>
    public bool Includes(MembershipDateRange range)
    {
        if (End is null)
        {
            return Start <= range.Start;
        }
        else if (range.End is null)
        {
            return false;
        }
        else
        {
            return (Start <= range.Start) && (range.End <= End);
        }
    }

    /// <summary>Overlapses the specified range.</summary>
    /// <param name="range">The range.</param>
    /// <returns></returns>
    public bool Overlaps(MembershipDateRange range)
    {
        if (Includes(range) || range.Includes(this))
        {
            return true;
        }
        else if (End is null & range.End is null)
        {
            return true;
        }
        else if (End is null)
        {
            return Start <= range.End;
        }
        else if (range.End is null)
        {
            return range.Start <= End;
        }
        else
        {
            return (Start <= range.Start && range.Start <= End) 
                ||(Start <= range.End && range.End <= End);
        }
    }

    /// <summary>Gets the equality components.</summary>
    /// <returns></returns>
    protected override IEnumerable<IComparable?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
