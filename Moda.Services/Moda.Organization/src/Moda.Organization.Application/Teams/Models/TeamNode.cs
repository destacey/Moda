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

    public ICollection<TeamMembershipEdge> ParentMemberships { get; set; } = [];
    public ICollection<TeamMembershipEdge> ChildMemberships { get; set; } = [];

    public static TeamNode Create(BaseTeam team)
    {
        return new TeamNode
        {
            Id = team.Id,
            Key = team.Key,
            Name = team.Name,
            Code = team.Code.Value,
            Type = team.Type,
            ActiveDate = team.ActiveDate,
            InactiveDate = team.InactiveDate,
            IsActive = team.IsActive
        };
    }
}
