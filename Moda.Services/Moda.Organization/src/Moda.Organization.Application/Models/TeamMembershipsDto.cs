using NodaTime;

namespace Moda.Organization.Application.Models;
public record TeamMembershipsDto
{
    public Guid Id { get; set; }
    public required TeamNavigationDto Child { get; set; }
    public required TeamNavigationDto Parent { get; set; }
    public LocalDate Start { get; set; }
    public LocalDate? End { get; set; }
    public required string State { get; set; }

    // TODO: do this with Mapster
    public static TeamMembershipsDto Create(TeamMembership membership, IDateTimeProvider dateTimeManager)
    {
        return new TeamMembershipsDto()
        {
            Id = membership.Id,
            Child = TeamNavigationDto.FromBaseTeam(membership.Source),
            Parent = TeamNavigationDto.FromBaseTeam(membership.Target),
            Start = membership.DateRange.Start,
            End = membership.DateRange.End,
            State = membership.StateOn(dateTimeManager.Now.InUtc().Date).GetDisplayName()
        };
    }
}
