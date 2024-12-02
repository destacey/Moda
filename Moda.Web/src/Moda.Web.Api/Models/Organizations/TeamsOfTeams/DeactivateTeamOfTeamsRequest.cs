using Moda.Organization.Application.TeamsOfTeams.Commands;

namespace Moda.Web.Api.Models.Organizations.Teams;

public sealed record DeactivateTeamOfTeamsRequest
{
    public Guid Id { get; set; }

    public LocalDate InactiveDate { get; set; }

    public DeactivateTeamOfTeamsCommand ToDeactivateTeamOfTeamsCommand()
    {
        return new DeactivateTeamOfTeamsCommand(Id, InactiveDate);
    }
}

public sealed class DeactivateTeamOfTeamsRequestValidator : CustomValidator<DeactivateTeamOfTeamsRequest>
{
    public DeactivateTeamOfTeamsRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Id)
            .NotEmpty();

        RuleFor(t => t.InactiveDate)
            .NotEmpty();
    }
}
