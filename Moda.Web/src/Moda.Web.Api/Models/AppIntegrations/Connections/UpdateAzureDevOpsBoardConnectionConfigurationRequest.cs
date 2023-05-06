namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record UpdateAzureDevOpsBoardConnectionConfigurationRequest
{
    /// <summary>Gets the connection identifier.</summary>
    /// <value>The connection identifier.</value>
    public Guid ConnectionId { get; set; }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string? Organization { get; set; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string? PersonalAccessToken { get; set; }

    public UpdateAzureDevOpsBoardsConnectionConfigurationCommand ToUpdateAzureDevOpsBoardsConnectionConfigurationCommand()
        => new(ConnectionId, Organization, PersonalAccessToken);
}

public sealed class UpdateAzureDevOpsBoardsConnectionConfigurationCommandValidator : CustomValidator<UpdateAzureDevOpsBoardsConnectionConfigurationCommand>
{
    public UpdateAzureDevOpsBoardsConnectionConfigurationCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Organization)
            .MaximumLength(256);

        RuleFor(c => c.PersonalAccessToken)
            .MaximumLength(256);
    }
}