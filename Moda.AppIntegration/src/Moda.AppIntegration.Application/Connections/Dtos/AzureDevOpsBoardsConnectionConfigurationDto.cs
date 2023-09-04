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

    /// <summary>Gets the organization URL.</summary>
    /// <value>The organization URL.</value>
    public string? OrganizationUrl { get; set; }

    public void MaskPersonalAccessToken()
    {
        if (!string.IsNullOrWhiteSpace(PersonalAccessToken) && PersonalAccessToken.Length > 4)
            PersonalAccessToken = PersonalAccessToken.Substring(0, 4) + new string('*', PersonalAccessToken.Length - 4);
    }
}
