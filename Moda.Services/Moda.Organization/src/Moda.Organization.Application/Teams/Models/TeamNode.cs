using Moda.Common.Domain.Enums.Organization;
using NodaTime;

namespace Moda.Organization.Application.Teams.Models;
public sealed record TeamNode
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public TeamType Type { get; set; }
    public LocalDate ActiveDate { get; set; }
    public LocalDate? InactiveDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<TeamMembershipEdge> ParentMemberships { get; set; } = [];
    public ICollection<TeamMembershipEdge> ChildMemberships { get; set; } = [];
}
