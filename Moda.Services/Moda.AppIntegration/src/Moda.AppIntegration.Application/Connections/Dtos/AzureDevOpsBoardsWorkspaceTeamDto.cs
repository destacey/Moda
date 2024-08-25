using Mapster;

namespace Moda.AppIntegration.Application.Connections.Dtos;
public sealed record AzureDevOpsBoardsWorkspaceTeamDto : IMapFrom<AzureDevOpsBoardsWorkspaceTeam>
{
    /// <summary>
    /// The unique identifier for the workspace in the Azure DevOps Boards system.
    /// </summary>
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// The unique identifier for the team in the Azure DevOps Boards system.
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// The name of the team in the Azure DevOps Boards system.
    /// </summary>
    public required string TeamName { get; set; }

    /// <summary>
    /// The unique identifier for the board in the Azure DevOps Boards system.
    /// </summary>
    public Guid? BoardId { get; set; }

    /// <summary>
    /// The unique identifier for the team within Moda.
    /// </summary>
    public Guid? InternalTeamId { get; set; }
}
