using Moda.Organization.Application.TeamsOfTeams.Commands;
using Moda.Organization.Domain.Extensions;
using Moda.Organization.Domain.Models;

namespace Moda.Web.Api.Models.Organizations.TeamsOfTeams;

public sealed record CreateTeamOfTeamsRequest
{
    /// <summary>Gets the team name.</summary>
    /// <value>The team name.</value>
    public required string Name { get; set; }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public required string Code { get; set; }

    /// <summary>Gets the team description.</summary>
    /// <value>The team description.</value>
    public string? Description { get; set; }

    /// <summary>
    /// The active date for the team.
    /// </summary>
    public LocalDate ActiveDate { get; set; }

    public CreateTeamOfTeamsCommand ToCreateTeamOfTeamsCommand()
    {
        TeamCode code = (TeamCode)Code;

        return new CreateTeamOfTeamsCommand(Name, code, Description, ActiveDate);
    }
}

public sealed class CreateTeamOfTeamsRequestValidator : CustomValidator<CreateTeamOfTeamsRequest>
{
    public CreateTeamOfTeamsRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(t => t.Code)
            .MinimumLength(2)
            .MaximumLength(10)
            .Must(t => t.IsValidTeamCodeFormat())
                .WithMessage("Invalid code format. Team codes are uppercase letters and numbers only, 2-10 characters.");

        RuleFor(t => t.Description)
            .MaximumLength(1024);

        RuleFor(t => t.ActiveDate)
            .NotEmpty();
    }
}
