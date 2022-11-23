namespace Moda.AppIntegration.Application.Connectors.Dtos;
public sealed record AzureDevOpsBoardsConnectorConfigurationDto
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

    public string? OrganizationUrl { get; set; }
}
