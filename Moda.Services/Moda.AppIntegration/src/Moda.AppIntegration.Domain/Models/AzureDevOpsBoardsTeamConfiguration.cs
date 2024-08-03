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

    /// <summary>
    /// Removes teams from the configuration.
    /// </summary>
    /// <param name="teamIds"></param>
    /// <returns></returns>
    public Result RemoveTeams(IEnumerable<Guid> teamIds)
    {
        try
        {
            if (teamIds is null || !teamIds.Any())
                return Result.Success();

            WorkspaceTeams.RemoveAll(w => teamIds.Contains(w.TeamId));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// Removes teams from the configuration for a specific workspace.
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <returns></returns>
    public Result RemoveTeamsForWorkspace(Guid workspaceId)
    {
        try
        {
            WorkspaceTeams.RemoveAll(w => w.WorkspaceId == workspaceId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsTeamConfiguration CreateEmpty()
    {
        return new AzureDevOpsBoardsTeamConfiguration();
    }
}
