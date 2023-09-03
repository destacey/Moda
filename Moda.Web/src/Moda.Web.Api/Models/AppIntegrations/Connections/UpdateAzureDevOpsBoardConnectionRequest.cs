namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record UpdateAzureDevOpsBoardConnectionRequest
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description { get; set; }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string? Organization { get; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string? PersonalAccessToken { get; }

    public UpdateAzureDevOpsBoardsConnectionCommand ToUpadateAzureDevOpsBoardsConnectionCommand()
        => new(Id, Name, Description, Organization, PersonalAccessToken);
}

public sealed class UpdateAzureDevOpsBoardConnectionRequestValidator : CustomValidator<UpdateAzureDevOpsBoardConnectionRequest>
{
    public UpdateAzureDevOpsBoardConnectionRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.Organization)
            .MaximumLength(128);

        RuleFor(c => c.PersonalAccessToken)
            .MaximumLength(128);
    }
}
