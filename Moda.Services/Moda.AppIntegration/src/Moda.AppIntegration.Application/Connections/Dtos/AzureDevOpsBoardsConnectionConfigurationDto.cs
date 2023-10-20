using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsConnectionConfigurationDto : IMapFrom<AzureDevOpsBoardsConnectionConfiguration>
{
    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public required string Organization { get; set; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public required string PersonalAccessToken { get; set; }

    /// <summary>Gets the organization URL.</summary>
    /// <value>The organization URL.</value>
    public required string OrganizationUrl { get; set; }

    /// <summary>Gets or sets the workspaces.</summary>
    /// <value>The workspaces.</value>
    public required List<AzureDevOpsBoardsWorkspaceDto> Workspaces { get; set; }

    public required List<AzureDevOpsBoardsWorkProcessDto> WorkProcesses { get; set; }

    public void MaskPersonalAccessToken()
    {
        if (!string.IsNullOrWhiteSpace(PersonalAccessToken) && PersonalAccessToken.Length > 4)
            PersonalAccessToken = string.Concat(PersonalAccessToken.AsSpan(0, 4), new string('*', PersonalAccessToken.Length - 4));
    }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<AzureDevOpsBoardsConnectionConfiguration, AzureDevOpsBoardsConnectionDetailsDto>();
    }
}
