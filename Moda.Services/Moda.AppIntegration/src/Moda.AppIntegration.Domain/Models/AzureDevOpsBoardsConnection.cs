using Moda.Common.Domain.Models;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnection : Connection<AzureDevOpsBoardsConnectionConfiguration>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AzureDevOpsBoardsConnection() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private AzureDevOpsBoardsConnection(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid)
    {
        Name = name;
        Description = description;
        Connector = Connector.AzureDevOpsBoards;
        Configuration = Guard.Against.Null(configuration, nameof(Configuration));
        IsValidConfiguration = configurationIsValid;
    }

    public override AzureDevOpsBoardsConnectionConfiguration Configuration { get; protected set; }

    public override bool HasActiveIntegrationObjects => IsValidConfiguration 
        && (Configuration.WorkProcesses.Any(p => p.IntegrationIsActive)
        || Configuration.Workspaces.Any(p => p.IntegrationIsActive));

    public Result Update(string name, string? description, string organization, string personalAccessToken, bool configurationIsValid, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));
            Guard.Against.Null(organization, nameof(organization)); 
            Guard.Against.Null(personalAccessToken, nameof(personalAccessToken));

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

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result UpdateWorkProcessIntegrationState(Guid workProcessExternalId, IntegrationState<Guid> integrationState, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));

            var workProcess = Configuration.WorkProcesses.FirstOrDefault(wp => wp.ExternalId == workProcessExternalId);
            if (workProcess is null)
                return Result.Failure($"Unable to find work process with id {workProcessExternalId} in Azure DevOps Boards connection with id {Id}.");

            if (workProcess.HasIntegration)
            {
                workProcess.UpdateIntegrationState(integrationState.IsActive);
            }
            else
            {
                workProcess.AddIntegrationState(integrationState);
            }

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsConnection Create(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid, Instant timestamp)
    {
        var connector = new AzureDevOpsBoardsConnection(name, description, configuration, configurationIsValid);
        connector.AddDomainEvent(EntityCreatedEvent.WithEntity(connector, timestamp));
        return connector;
    }
}
