using Moda.Common.Domain.Extensions.Organizations;
using Moda.Common.Domain.Models.Organizations;
using Moda.Organization.Application.TeamsOfTeams.Commands;

namespace Moda.Web.Api.Models.Organizations.TeamOfTeams;

public sealed record UpdateTeamOfTeamsRequest
{
    public Guid Id { get; set; }

    /// <summary>Gets the team name.</summary>
    /// <value>The team name.</value>
    public string Name { get; set; } = default!;

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public string Code { get; set; } = default!;

    /// <summary>Gets the team description.</summary>
    /// <value>The team description.</value>
    public string? Description { get; set; }

    public UpdateTeamOfTeamsCommand ToUpdateTeamOfTeamsCommand()
    {
        return new UpdateTeamOfTeamsCommand(Id, Name, (TeamCode)Code, Description);
    }
}

public sealed class UpdateTeamOfTeamsRequestValidator : CustomValidator<UpdateTeamOfTeamsRequest>
{
    public UpdateTeamOfTeamsRequestValidator()
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
                .WithMessage("Invalid code format. Team codes are uppercase letters and numbers only, 2-10 characters.");

        RuleFor(t => t.Description)
            .MaximumLength(1024);
    }
}
