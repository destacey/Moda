namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record CreateAzureDevOpsConnectionRequest
{
    /// <summary>
    /// The name of the connection.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The description of the connection.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The Azure DevOps Organization name.
    /// </summary>
    public string Organization { get; set; } = default!;

    /// <summary>
    /// The personal access token that enables access to Azure DevOps Boards data.
    /// </summary>
    public string PersonalAccessToken { get; set; } = default!;

    public CreateAzureDevOpsBoardsConnectionCommand ToCreateAzureDevOpsBoardsConnectionCommand()
        => new(Name, Description, Organization, PersonalAccessToken);
}

public sealed class CreateAzureDevOpsConnectionRequestValidator : CustomValidator<CreateAzureDevOpsConnectionRequest>
{
    public CreateAzureDevOpsConnectionRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.Organization)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.PersonalAccessToken)
            .NotEmpty()
            .MaximumLength(128);
    }
}
