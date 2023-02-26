using FluentValidation;
using Moda.Organization.Application.Teams.Commands;
using Moda.Organization.Domain.Models;
using Moda.Organization.Domain.Extensions;

namespace Moda.Web.Api.Models.Organizations.Teams;

public sealed record CreateTeamRequest
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

    public CreateTeamCommand ToCreateTeamCommand()
    {
        TeamCode code = (TeamCode)Code;

        return new CreateTeamCommand(Name, code, Description);
    }
}

public sealed class CreateTeamRequestValidator : CustomValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(t => t.Code)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(10)
            .Must(t => t.IsValidTeamCodeFormat())
                .WithMessage("Invalid code format. Team codes are uppercase letters only, 2-10 characters.");

        RuleFor(t => t.Description)
            .MaximumLength(1024);
    }
}
