namespace Moda.Organization.Domain.Models;
public sealed class TeamToTeamMembership : BaseMembership
{
    private TeamToTeamMembership() { }

    private TeamToTeamMembership(Guid sourceId, Guid targetId, MembershipDateRange dateRange)
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
    public BaseTeam Source { get; private set; } = null!;

    /// <summary>Gets the target or parent team of teams.</summary>
    /// <value>The target.</value>
    public BaseTeam Target { get; private set; } = null!;

    internal static TeamToTeamMembership Create(Guid childId, Guid parentId, MembershipDateRange dateRange)
    {
        return new TeamToTeamMembership(childId, parentId, dateRange);
    }
}
