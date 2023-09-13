namespace Moda.Organization.Domain.Models;
public sealed class TeamMembership : BaseMembership
{
    private TeamMembership() { }

    private TeamMembership(Guid sourceId, Guid targetId, MembershipDateRange dateRange)
    {
        if (sourceId == targetId)
        {
            throw new ArgumentException("A team or team of teams cannot have a membership with its self.");
        }

        SourceId = sourceId;
        TargetId = targetId;
        DateRange = dateRange;
    }

    /// <summary>Gets the source or child team or team of teams.</summary>
    /// <value>The source.</value>
    public BaseTeam Source { get; private set; } = default!;

    /// <summary>Gets the target or parent team of teams.</summary>
    /// <value>The target.</value>
    public TeamOfTeams Target { get; private set; } = default!;

    internal static TeamMembership Create(Guid childId, Guid parentId, MembershipDateRange dateRange)
    {
        return new TeamMembership(childId, parentId, dateRange);
    }
}
