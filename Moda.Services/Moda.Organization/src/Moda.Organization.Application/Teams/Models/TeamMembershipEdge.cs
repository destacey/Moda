namespace Moda.Organization.Application.Teams.Models;
public sealed record TeamMembershipEdge
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public TeamNode FromNode { get; set; } = null!;
    public TeamNode ToNode { get; set; } = null!;
}
