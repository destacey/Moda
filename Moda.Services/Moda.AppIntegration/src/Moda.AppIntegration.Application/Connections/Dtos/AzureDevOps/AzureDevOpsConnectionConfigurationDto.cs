using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos.AzureDevOps;
public sealed record AzureDevOpsConnectionConfigurationDto : IMapFrom<AzureDevOpsBoardsConnectionConfiguration>
{
    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public required string Organization { get; set; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps data.</value>
    public required string PersonalAccessToken { get; set; }

    /// <summary>Gets the organization URL.</summary>
    /// <value>The organization URL.</value>
    public required string OrganizationUrl { get; set; }

    /// <summary>
    /// Gets or sets the work processes.
    /// </summary>
    public required List<AzureDevOpsWorkProcessDto> WorkProcesses { get; set; }

    /// <summary>
    /// Gets or sets the workspaces.
    /// </summary>
    public required List<AzureDevOpsWorkspaceDto> Workspaces { get; set; }

    public void MaskPersonalAccessToken()
    {
        if (!string.IsNullOrWhiteSpace(PersonalAccessToken) && PersonalAccessToken.Length > 4)
            PersonalAccessToken = string.Concat(PersonalAccessToken.AsSpan(0, 4), new string('*', PersonalAccessToken.Length - 4));
    }
}
