using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsTeamConfigurationDto : IMapFrom<AzureDevOpsBoardsTeamConfiguration>
{
    /// <summary>
    /// Teams that are associated with the workspace.
    /// </summary>
    public List<AzureDevOpsBoardsWorkspaceTeamDto> WorkspaceTeams { get; set; } = [];
}
