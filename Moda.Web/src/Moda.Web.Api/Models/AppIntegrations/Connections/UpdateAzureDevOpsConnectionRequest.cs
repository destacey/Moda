using Moda.AppIntegration.Application.Connections.Commands.AzureDevOps;

namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record UpdateAzureDevOpsConnectionRequest : UpdateConnectionRequest
{
    /// <summary>
    /// The Azure DevOps Organization name.
    /// </summary>
    public required string Organization { get; set; }

    /// <summary>
    /// The personal access token that enables access to Azure DevOps data.
    /// </summary>
    public required string PersonalAccessToken { get; set; }

    public UpdateAzureDevOpsConnectionCommand ToCommand()
        => new(Id, Name, Description, Organization, PersonalAccessToken);
}

public sealed class UpdateAzureDevOpsConnectionRequestValidator : CustomValidator<UpdateAzureDevOpsConnectionRequest>
{
    public UpdateAzureDevOpsConnectionRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new UpdateConnectionRequestValidator());

        RuleFor(c => c.Organization)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.PersonalAccessToken)
            .NotEmpty()
            .MaximumLength(128);
    }
}
