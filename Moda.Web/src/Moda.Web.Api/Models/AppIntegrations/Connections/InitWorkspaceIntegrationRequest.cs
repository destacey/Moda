using Moda.Work.Domain.Extensions;

namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record InitWorkspaceIntegrationRequest
{
    /// <summary>
    /// Connection Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// External identifier for the workspace.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// The key for the workspace.
    /// </summary>
    public required string WorkspaceKey { get; set; }
}

public sealed class InitWorkspaceIntegrationRequestValidator : CustomValidator<InitWorkspaceIntegrationRequest>
{
    public InitWorkspaceIntegrationRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Id)
            .NotEmpty();

        RuleFor(t => t.ExternalId)
            .NotEmpty();

        RuleFor(t => t.WorkspaceKey)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(20)
            .Must(t => t.IsValidWorkspaceKeyFormat())
                .WithMessage("Invalid workspace key format. Workspace key's must be uppercase letters and numbers only and start with an uppercase letter.");
    }
}
