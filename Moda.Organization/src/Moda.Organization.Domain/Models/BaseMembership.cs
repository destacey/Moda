using Ardalis.GuardClauses;
using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public abstract class BaseMembership : BaseAuditableEntity<Guid>
{
    private MembershipDateRange _dateRange = default!;

    /// <summary>Gets the source identifier.</summary>
    /// <value>The source identifier.</value>
    public Guid SourceId { get; init; }

    /// <summary>Gets the target identifier.</summary>
    /// <value>The target identifier.</value>
    public Guid TargetId { get; init; }

    /// <summary>Gets or sets the date range.</summary>
    /// <value>The date range.</value>
    public MembershipDateRange DateRange
    {
        get => _dateRange;
        protected set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    /// <summary>Updates the specified date range.</summary>
    /// <param name="dateRange">The date range.</param>
    public void Update(MembershipDateRange dateRange)
    {
        DateRange = dateRange;
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
        return DateRange.Includes(date);
    }

    /// <summary>States the on.</summary>
    /// <param name="date">The date.</param>
    /// <returns></returns>
    public MembershipState StateOn(LocalDate date)
    {
        if (DateRange.End < date) { return MembershipState.Past; }
        if (DateRange.Includes(date)) { return MembershipState.Active; };
        return MembershipState.Future;
    }
}
