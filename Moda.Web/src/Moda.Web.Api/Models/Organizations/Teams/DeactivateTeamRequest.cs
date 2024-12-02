using Moda.Organization.Application.Teams.Commands;

namespace Moda.Web.Api.Models.Organizations.Teams;

public sealed record DeactivateTeamRequest
{
    public Guid Id { get; set; }

    public LocalDate InactiveDate { get; set; }

    public DeactivateTeamCommand ToDeactivateTeamCommand()
    {
        return new DeactivateTeamCommand(Id, InactiveDate);
    }
}

public sealed class DeactivateTeamRequestValidator : CustomValidator<DeactivateTeamRequest>
{
    public DeactivateTeamRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Id)
            .NotEmpty();

        RuleFor(t => t.InactiveDate)
            .NotEmpty();
    }
}
