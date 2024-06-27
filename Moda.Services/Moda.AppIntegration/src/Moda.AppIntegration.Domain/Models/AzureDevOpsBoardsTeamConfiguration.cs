namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsTeamConfiguration
{
    private AzureDevOpsBoardsTeamConfiguration() { }

    public AzureDevOpsBoardsTeamConfiguration(IEnumerable<AzureDevOpsBoardsWorkspaceTeam> workspaceTeams)
    {
        WorkspaceTeams = workspaceTeams?.ToList() ?? [];
    }

    /// <summary>
    /// Gets or sets the workspace teams.
    /// </summary>
    public List<AzureDevOpsBoardsWorkspaceTeam> WorkspaceTeams { get; private set; } = [];

    public Result UpsertWorkspaceTeam(Guid workspaceId, Guid teamId, string teamName)
    {
        try
        {
            Guard.Against.Default(workspaceId, nameof(workspaceId));
            Guard.Against.Default(teamId, nameof(teamId));
            Guard.Against.NullOrWhiteSpace(teamName, nameof(teamName));

            var existingTeam = WorkspaceTeams.SingleOrDefault(w => w.WorkspaceId == workspaceId && w.TeamId == teamId);
            if (existingTeam is null)
            {
                WorkspaceTeams.Add(new AzureDevOpsBoardsWorkspaceTeam(workspaceId, teamId, teamName));
            }
            else
            {
                existingTeam.Update(teamName);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    // TODO: Add method to remove workspace team
    // TODO: Add method to remove workspace

    public static AzureDevOpsBoardsTeamConfiguration CreateEmpty()
    {
        return new AzureDevOpsBoardsTeamConfiguration();
    }
}
