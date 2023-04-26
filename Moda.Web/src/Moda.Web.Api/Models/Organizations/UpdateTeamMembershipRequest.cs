using FluentValidation;
using Moda.Organization.Domain.Models;
using NodaTime;

namespace Moda.Web.Api.Models.Organizations;

public sealed record UpdateTeamMembershipRequest
{
    public Guid TeamId { get; set; }
    public Guid TeamMembershipId { get; set; }
    public LocalDate Start { get; set; }
    public LocalDate? End { get; set; }

    public Organization.Application.Teams.Commands.UpdateTeamMembershipCommand ToTeamUpdateTeamMembershipCommand()
    {
        return new Organization.Application.Teams.Commands.UpdateTeamMembershipCommand(TeamId, TeamMembershipId, new MembershipDateRange(Start, End));
    }

    public Organization.Application.TeamsOfTeams.Commands.UpdateTeamMembershipCommand ToTeamOfTeamsUpdateTeamMembershipCommand()
    {
        return new Organization.Application.TeamsOfTeams.Commands.UpdateTeamMembershipCommand(TeamId, TeamMembershipId, new MembershipDateRange(Start, End));
    }
}

public sealed class UpdateTeamMembershipRequestValidator : CustomValidator<UpdateTeamMembershipRequest>
{
    public UpdateTeamMembershipRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.TeamId)
            .NotEmpty();

        RuleFor(t => t.TeamMembershipId)
            .NotEmpty();

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .Must((membership, end) => end == null || membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
