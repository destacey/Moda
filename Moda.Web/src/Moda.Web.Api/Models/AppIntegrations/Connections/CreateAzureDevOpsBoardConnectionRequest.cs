namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record CreateAzureDevOpsBoardConnectionRequest
{
    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description { get; set; }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public required string Organization { get; set; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public required string PersonalAccessToken { get; set; }

    public CreateAzureDevOpsBoardsConnectionCommand ToCreateAzureDevOpsBoardsConnectionCommand()
        => new(Name, Description, Organization, PersonalAccessToken);
}

public sealed class CreateAzureDevOpsBoardConnectionRequestValidator : CustomValidator<CreateAzureDevOpsBoardConnectionRequest>
{
    public CreateAzureDevOpsBoardConnectionRequestValidator()
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
