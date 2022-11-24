namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsConnectionConfigurationDto
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

    public string? OrganizationUrl { get; set; }
}
