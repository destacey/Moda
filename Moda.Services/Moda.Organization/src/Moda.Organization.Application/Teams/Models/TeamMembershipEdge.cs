using NodaTime;

namespace Moda.Organization.Application.Teams.Models;
public sealed record TeamMembershipEdge
{
    public Guid Id { get; set; }
    public LocalDate StartDate { get; set; }
    public LocalDate? EndDate { get; set; }

    public TeamNode FromNode { get; set; } = null!;
    public TeamNode ToNode { get; set; } = null!;
}
