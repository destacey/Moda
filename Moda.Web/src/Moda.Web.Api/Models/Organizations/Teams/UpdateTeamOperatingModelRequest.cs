using Moda.Organization.Application.Teams.Commands;
using Moda.Organization.Domain.Enums;

namespace Moda.Web.Api.Models.Organizations.Teams;

public sealed record UpdateTeamOperatingModelRequest
{
    /// <summary>
    /// The methodology the team uses (e.g., Scrum, Kanban).
    /// </summary>
    public Methodology Methodology { get; set; }

    /// <summary>
    /// The sizing method the team uses (e.g., StoryPoints, Count).
    /// </summary>
    public SizingMethod SizingMethod { get; set; }

    public UpdateTeamOperatingModelCommand ToUpdateTeamOperatingModelCommand(Guid teamId, Guid operatingModelId)
    {
        return new UpdateTeamOperatingModelCommand(teamId, operatingModelId, Methodology, SizingMethod);
    }
}

public sealed class UpdateTeamOperatingModelRequestValidator : CustomValidator<UpdateTeamOperatingModelRequest>
{
    public UpdateTeamOperatingModelRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.Methodology)
            .IsInEnum();

        RuleFor(r => r.SizingMethod)
            .IsInEnum();
    }
}
