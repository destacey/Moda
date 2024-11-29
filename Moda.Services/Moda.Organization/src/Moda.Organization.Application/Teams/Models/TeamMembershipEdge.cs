using NodaTime;

namespace Moda.Organization.Application.Teams.Models;
public sealed record TeamMembershipEdge
{
    public Guid Id { get; set; }
    public LocalDate StartDate { get; set; }
    public LocalDate? EndDate { get; set; }

    /// <summary>
    /// The child team in the relationship.
    /// </summary>
    public TeamNode FromNode { get; set; } = null!;

    /// <summary>
    /// The parent team in the relationship.    
    /// </summary>
    public TeamNode ToNode { get; set; } = null!;

    public static TeamMembershipEdge From(TeamMembership membership)
    {
        return new TeamMembershipEdge
        {
            Id = membership.Id,
            StartDate = membership.DateRange.Start,
            EndDate = membership.DateRange.End,
            FromNode = TeamNode.From(membership.Source),
            ToNode = TeamNode.From(membership.Target)
        };
    }
}
