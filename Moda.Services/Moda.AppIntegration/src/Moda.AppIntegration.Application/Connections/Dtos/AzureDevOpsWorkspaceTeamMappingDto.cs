using FluentValidation;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsWorkspaceTeamMappingDto(Guid WorkspaceId, Guid ExternalTeamId, Guid? InternalTeamId);

public sealed class AzureDevOpsWorkspaceTeamMappingDtoValidator : CustomValidator<AzureDevOpsWorkspaceTeamMappingDto>
{
    public AzureDevOpsWorkspaceTeamMappingDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.WorkspaceId)
            .NotEmpty();

        RuleFor(t => t.ExternalTeamId)
            .NotEmpty();
    }
}
