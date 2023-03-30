using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public abstract class BaseMembership : BaseAuditableEntity<Guid>
{
    public Guid SourceId { get; protected set; }
    public Guid TargetId { get; protected set; }
    public MembershipType Type { get; protected set; }
    public MembershipDateRange DateRange { get; protected set; } = default!;

    public bool IsActiveOn(LocalDate date)
    {
        return DateRange.Includes(date);
    }

    public MembershipState StateOn(LocalDate date)
    {
        if (DateRange.End < date) { return MembershipState.Past; }
        if (DateRange.Includes(date)) { return MembershipState.Active; };
        return MembershipState.Future;
    }
}
