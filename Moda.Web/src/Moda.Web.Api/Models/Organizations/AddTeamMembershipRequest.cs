using Moda.Organization.Domain.Models;

namespace Moda.Web.Api.Models.Organizations;

public sealed record AddTeamMembershipRequest
{
    public Guid TeamId { get; set; }
    public Guid ParentTeamId { get; set; }
    public LocalDate Start { get; set; }
    public LocalDate? End { get; set; }

    public Organization.Application.Teams.Commands.AddTeamMembershipCommand ToTeamAddParentTeamMembershipCommand()
    {
        return new Organization.Application.Teams.Commands.AddTeamMembershipCommand(TeamId, ParentTeamId, new MembershipDateRange(Start, End));
    }

    public Organization.Application.TeamsOfTeams.Commands.AddTeamMembershipCommand ToTeamOfTeamsAddParentTeamMembershipCommand()
    {
        return new Organization.Application.TeamsOfTeams.Commands.AddTeamMembershipCommand(TeamId, ParentTeamId, new MembershipDateRange(Start, End));
    }
}

public sealed class TeamMembershipRequestValidator : CustomValidator<AddTeamMembershipRequest>
{
    public TeamMembershipRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.TeamId)
            .NotEmpty();

        RuleFor(t => t.ParentTeamId)
            .NotEmpty();

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .Must((membership, end) => end == null || membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
