using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;
using Moda.Common.Extensions;
using Moda.Common.Models;
using Moda.Planning.Domain.Enums;
using NodaTime;

namespace Moda.Planning.Domain.Models;
public class ProgramIncrement : BaseAuditableEntity<Guid>
{
    private string _name = null!;
    private string? _description;
    private LocalDateRange _dateRange = default!;

    private ProgramIncrement() { }

    private ProgramIncrement(string name, string? description, LocalDateRange dateRange)
    {
        Name = name;
        Description = description;
        DateRange = dateRange;
    }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; private set; }

    /// <summary>
    /// The name of the Program Increment.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the Program Increment.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets or sets the date range.</summary>
    /// <value>The date range.</value>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        protected set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    /// <summary>Updates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="dateRange">The date range.</param>
    public Result Update(string name, string? description, LocalDateRange dateRange)
    {
        try
        {
            Name = name;
            Description = description;
            DateRange = dateRange;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>States the on.</summary>
    /// <param name="date">The date.</param>
    /// <returns></returns>
    public IterationState StateOn(LocalDate date)
    {
        if (DateRange.IsPastOn(date)) { return IterationState.Completed; }
        if (DateRange.IsActiveOn(date)) { return IterationState.Active; };
        return IterationState.Future;
    }

    /// <summary>Creates the specified name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="dateRange">The date range.</param>
    /// <returns></returns>
    public static ProgramIncrement Create(string name, string? description, LocalDateRange dateRange)
    {
        return new ProgramIncrement(name, description, dateRange);
    }
}
