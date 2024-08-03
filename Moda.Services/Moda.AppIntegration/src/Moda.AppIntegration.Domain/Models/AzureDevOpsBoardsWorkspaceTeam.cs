namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkspaceTeam
{
    private string _teamName = default!;

    private AzureDevOpsBoardsWorkspaceTeam() { }

    public AzureDevOpsBoardsWorkspaceTeam(Guid workspaceId, Guid teamId, string teamName)
    {
        WorkspaceId = workspaceId;
        TeamId = teamId;
        TeamName = teamName;
    }

    /// <summary>
    /// WorkspaceId is the unique identifier for the project in the Azure DevOps Boards system.
    /// </summary>
    public Guid WorkspaceId { get; private init; }

    /// <summary>
    /// TeamId is the unique identifier for the team in the Azure DevOps Boards system.
    /// </summary>
    public Guid TeamId { get; private init; }

    /// <summary>
    /// TeamName is the name of the team in the Azure DevOps Boards system.
    /// </summary>
    public string TeamName 
    { 
        get => _teamName; 
        private set => _teamName = Guard.Against.NullOrWhiteSpace(value, nameof(TeamName)); 
    }

    /// <summary>
    /// InternalTeamId is the unique identifier for the team in the Moda system.
    /// </summary>
    public Guid? InternalTeamId { get; private set; }

    public void Update(string teamName)
    {
        TeamName = teamName;
    }

    public void MapInternalTeam(Guid? internalTeamId)
    {
        InternalTeamId = internalTeamId;
    }
}
