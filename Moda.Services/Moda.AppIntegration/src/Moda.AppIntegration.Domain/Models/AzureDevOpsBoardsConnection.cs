using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.AppIntegrations;
using Moda.Common.Domain.Models;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnection : Connection<AzureDevOpsBoardsConnectionConfiguration, AzureDevOpsBoardsTeamConfiguration>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AzureDevOpsBoardsConnection() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private AzureDevOpsBoardsConnection(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid, AzureDevOpsBoardsTeamConfiguration? teamConfiguration)
    {
        Name = name;
        Description = description;
        Connector = Connector.AzureDevOpsBoards;
        Configuration = Guard.Against.Null(configuration, nameof(Configuration));
        IsValidConfiguration = configurationIsValid;
        TeamConfiguration = teamConfiguration ?? AzureDevOpsBoardsTeamConfiguration.CreateEmpty();
    }

    public override AzureDevOpsBoardsConnectionConfiguration Configuration { get; protected set; }

    public override AzureDevOpsBoardsTeamConfiguration TeamConfiguration { get; protected set; }

    public override bool HasActiveIntegrationObjects => IsValidConfiguration
        && (Configuration.WorkProcesses.Any(p => p.IntegrationIsActive)
        || Configuration.Workspaces.Any(p => p.IntegrationIsActive));

    public Result Update(string name, string? description, string organization, string personalAccessToken, bool configurationIsValid, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));
            Guard.Against.NullOrWhiteSpace(organization, nameof(organization));
            Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));

            Name = name;
            Description = description;
            IsValidConfiguration = configurationIsValid;

            Configuration.Organization = organization;
            Configuration.PersonalAccessToken = personalAccessToken;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result SyncWorkspaces(IEnumerable<AzureDevOpsBoardsWorkspace> workspaces, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));

            // remove workspaces that are not in the new list
            var workspacesToRemove = Configuration.Workspaces.Where(w => !workspaces.Any(nw => nw.ExternalId == w.ExternalId)).ToList();
            foreach (var workspace in workspacesToRemove)
            {
                // TODO - what if the workspace had been configured to sync and has data?
                var result = Configuration.RemoveWorkspace(workspace);
                if (result.IsFailure)
                    return result;

                // remove any teams associated with the workspace
                var removeTeamsResult = TeamConfiguration.RemoveTeamsForWorkspace(workspace.ExternalId);
            }

            // update existing or add new workspaces
            foreach (var workspace in workspaces)
            {
                var existingWorkspace = Configuration.Workspaces.FirstOrDefault(w => w.ExternalId == workspace.ExternalId);
                if (existingWorkspace is not null)
                {
                    var result = existingWorkspace.Update(
                        workspace.Name,
                        workspace.Description,
                        workspace.WorkProcessId);

                    if (result.IsFailure)
                        return result;
                }
                else
                {
                    var result = Configuration.AddWorkspace(AzureDevOpsBoardsWorkspace.Create(
                        workspace.ExternalId,
                        workspace.Name,
                        workspace.Description,
                        workspace.WorkProcessId));

                    if (result.IsFailure)
                        return result;
                }
            }

            // TODO this will generate duplicate events in some cases
            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result SyncProcesses(IEnumerable<AzureDevOpsBoardsWorkProcess> processes, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));

            // remove processes that are not in the new list
            var processesToRemove = Configuration.WorkProcesses.Where(w => !processes.Any(nw => nw.ExternalId == w.ExternalId)).ToList();
            foreach (var process in processesToRemove)
            {
                // TODO - what if the process had been configured to sync and has data?
                var result = Configuration.RemoveWorkProcess(process);
                if (result.IsFailure)
                    return result;
            }

            // update existing or add new processes
            foreach (var process in processes)
            {
                var existingProcess = Configuration.WorkProcesses.FirstOrDefault(w => w.ExternalId == process.ExternalId);
                if (existingProcess is not null)
                {
                    var result = existingProcess.Update(
                        process.Name,
                        process.Description);

                    if (result.IsFailure)
                        return result;
                }
                else
                {
                    var result = Configuration.AddWorkProcess(AzureDevOpsBoardsWorkProcess.Create(
                        process.ExternalId,
                        process.Name,
                        process.Description));

                    if (result.IsFailure)
                        return result;
                }
            }

            // TODO this will generate duplicate events in some cases
            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result SyncTeams(List<IExternalTeam> teams, Instant timestamp)
    {
        try
        {
            TeamConfiguration ??= AzureDevOpsBoardsTeamConfiguration.CreateEmpty();

            var teamsToRemove = TeamConfiguration.WorkspaceTeams
                .Where(t => !teams.Any(nw => nw.Id == t.TeamId))
                .Select(t => t.TeamId)
                .ToArray();
            var removeResult = TeamConfiguration.RemoveTeams(teamsToRemove);
            if (removeResult.IsFailure)
                return removeResult;

            foreach (var team in teams)
            {
                var result = TeamConfiguration.UpsertWorkspaceTeam(team.WorkspaceId, team.Id, team.Name, team.BoardId);
                if (result.IsFailure)
                    return result;
            }

            // TODO this will generate duplicate events in some cases
            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result UpdateWorkProcessIntegrationState(IntegrationRegistration<Guid, Guid> registration, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));

            var workProcess = Configuration.WorkProcesses.FirstOrDefault(wp => wp.ExternalId == registration.ExternalId);
            if (workProcess is null)
                return Result.Failure($"Unable to find work process with id {registration.ExternalId} in Azure DevOps Boards connection with id {Id}.");

            Result setResult = workProcess.HasIntegration
                ? workProcess.UpdateIntegrationState(registration.IntegrationState.IsActive)
                : workProcess.AddIntegrationState(registration.IntegrationState);
            if (setResult.IsFailure)
                return setResult;

            // TODO this will generate duplicate events in some cases
            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result UpdateWorkspaceIntegrationState(IntegrationRegistration<Guid, Guid> registration, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));

            var workspace = Configuration.Workspaces.FirstOrDefault(wp => wp.ExternalId == registration.ExternalId);
            if (workspace is null)
                return Result.Failure($"Unable to find workspace with id {registration.ExternalId} in Azure DevOps Boards connection with id {Id}.");

            Result setResult = workspace.HasIntegration
                ? workspace.UpdateIntegrationState(registration.IntegrationState.IsActive)
                : workspace.AddIntegrationState(registration.IntegrationState);
            if (setResult.IsFailure)
                return setResult;

            // TODO this will generate duplicate events in some cases
            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsConnection Create(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid, AzureDevOpsBoardsTeamConfiguration? teamConfiguration, Instant timestamp)
    {
        var connector = new AzureDevOpsBoardsConnection(name, description, configuration, configurationIsValid, teamConfiguration);
        connector.AddDomainEvent(EntityCreatedEvent.WithEntity(connector, timestamp));
        return connector;
    }
}
