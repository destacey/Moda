namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record CreateAzureDevOpsConnectionRequest : CreateConnectionRequest
{
    /// <summary>
    /// The Azure DevOps Organization name.
    /// </summary>
    public required string Organization { get; set; }

    /// <summary>
    /// The personal access token that enables access to Azure DevOps data.
    /// </summary>
    public required string PersonalAccessToken { get; set; }

    public CreateAzureDevOpsConnectionCommand ToCommand()
        => new(Name, Description, Organization, PersonalAccessToken);
}

public sealed class CreateAzureDevOpsConnectionRequestValidator : CustomValidator<CreateAzureDevOpsConnectionRequest>
{
    public CreateAzureDevOpsConnectionRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new CreateConnectionRequestValidator());

        RuleFor(c => c.Organization)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.PersonalAccessToken)
            .NotEmpty()
            .MaximumLength(128);
    }
}
