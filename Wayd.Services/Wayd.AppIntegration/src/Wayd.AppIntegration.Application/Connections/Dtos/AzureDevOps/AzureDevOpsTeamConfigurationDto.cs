using Mapster;

namespace Wayd.AppIntegration.Application.Connections.Dtos.AzureDevOps;

public sealed record AzureDevOpsTeamConfigurationDto : IMapFrom<AzureDevOpsBoardsTeamConfiguration>
{
    /// <summary>
    /// Teams that are associated with the workspace.
    /// </summary>
    public List<AzureDevOpsWorkspaceTeamDto> WorkspaceTeams { get; set; } = [];
}
