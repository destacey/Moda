using System.Text.Json.Serialization;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkspaceTeam
{
    private string _teamName = default!;

    private AzureDevOpsBoardsWorkspaceTeam() { }

    [JsonConstructor]
    public AzureDevOpsBoardsWorkspaceTeam(Guid workspaceId, Guid teamId, string teamName, Guid? boardId)
    {
        WorkspaceId = workspaceId;
        TeamId = teamId;
        TeamName = teamName;
        BoardId = boardId;
    }

    /// <summary>
    /// WorkspaceId is the unique identifier for the project in the Azure DevOps system.
    /// </summary>
    public Guid WorkspaceId { get; private init; }

    /// <summary>
    /// TeamId is the unique identifier for the team in the Azure DevOps system.
    /// </summary>
    public Guid TeamId { get; private init; }

    /// <summary>
    /// TeamName is the name of the team in the Azure DevOps system.
    /// </summary>
    public string TeamName 
    { 
        get => _teamName; 
        private set => _teamName = Guard.Against.NullOrWhiteSpace(value, nameof(TeamName)); 
    }

    public Guid? BoardId { get; private set; }

    /// <summary>
    /// InternalTeamId is the unique identifier for the team in the Moda system.
    /// </summary>
    public Guid? InternalTeamId { get; private set; }

    public void Update(string teamName, Guid? boardId)
    {
        TeamName = teamName;
        BoardId = boardId;
    }

    public void MapInternalTeam(Guid? internalTeamId)
    {
        InternalTeamId = internalTeamId;
    }
}
