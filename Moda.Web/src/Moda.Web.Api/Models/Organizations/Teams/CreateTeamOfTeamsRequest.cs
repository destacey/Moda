using FluentValidation;
using Moda.Organization.Application.Teams.Commands;
using Moda.Organization.Domain.Models;

namespace Moda.Web.Api.Models.Organizations.Teams;

public sealed record CreateTeamOfTeamsRequest
{
    /// <summary>Gets the team name.</summary>
    /// <value>The team name.</value>
    public string Name { get; } = null!;

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public string Code { get; } = null!;

    /// <summary>Gets the team description.</summary>
    /// <value>The team description.</value>
    public string? Description { get; }

    public CreateTeamOfTeamsCommand ToCreateTeamOfTeamsCommand()
    {
        TeamCode code = (TeamCode)Code;

        return new CreateTeamOfTeamsCommand(Name, code, Description);
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
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(10);

        RuleFor(t => t.Description)
            .MaximumLength(1024);
    }
}
