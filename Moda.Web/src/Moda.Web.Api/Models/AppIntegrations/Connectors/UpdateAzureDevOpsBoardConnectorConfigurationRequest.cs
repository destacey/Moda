using FluentValidation;

namespace Moda.Web.Api.Models.AppIntegrations.Connectors;

public sealed record UpdateAzureDevOpsBoardConnectorConfigurationRequest
{
    /// <summary>Gets the connector identifier.</summary>
    /// <value>The connector identifier.</value>
    public Guid ConnectorId { get; set; }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string? Organization { get; set; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string? PersonalAccessToken { get; set; }

    public UpdateAzureDevOpsBoardsConnectorConfigurationCommand ToUpdateAzureDevOpsBoardsConnectorConfigurationCommand()
        => new(ConnectorId, Organization, PersonalAccessToken);
}

public sealed class UpdateAzureDevOpsBoardsConnectorConfigurationCommandValidator : CustomValidator<UpdateAzureDevOpsBoardsConnectorConfigurationCommand>
{
    public UpdateAzureDevOpsBoardsConnectorConfigurationCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Organization)
            .MaximumLength(256);

        RuleFor(c => c.PersonalAccessToken)
            .MaximumLength(256);
    }
}