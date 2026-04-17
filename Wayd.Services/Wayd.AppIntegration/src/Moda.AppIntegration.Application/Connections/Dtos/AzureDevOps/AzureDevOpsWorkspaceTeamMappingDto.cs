using FluentValidation;

namespace Wayd.AppIntegration.Application.Connections.Dtos.AzureDevOps;

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

        When(t => t.InternalTeamId.HasValue, () =>
        {
            RuleFor(t => t.InternalTeamId)
                .NotEmpty();
        });
        // TODO: Add verification for InternalTeamId.  Example: .MustAsync(BeValidTeamId).WithErrorCode("Invalid InternalTeamId");
    }
}
