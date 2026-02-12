using Moda.AppIntegration.Application.Connections.Dtos.AzureDevOps;

namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record AzdoWorkspaceTeamMappingRequest
{
    /// <summary>
    /// The unique identifier for the workspace in the Azure DevOps system.
    /// </summary>
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// The unique identifier for the team in the Azure DevOps system.
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// The unique identifier for the team within Moda.
    /// </summary>
    public Guid? InternalTeamId { get; set; }

    public AzureDevOpsWorkspaceTeamMappingDto ToAzureDevOpsWorkspaceTeamMappingDto()
    {
        return new AzureDevOpsWorkspaceTeamMappingDto(WorkspaceId, TeamId, InternalTeamId);
    }
}

public sealed class AzdoWorkspaceTeamMappingRequestValidator : CustomValidator<AzdoWorkspaceTeamMappingRequest>
{
    public AzdoWorkspaceTeamMappingRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.WorkspaceId)
            .NotEmpty();

        RuleFor(t => t.TeamId)
            .NotEmpty();

        When(t => t.InternalTeamId.HasValue, 
            () => RuleFor(t => t.InternalTeamId)
                .NotEmpty());
    }
}
