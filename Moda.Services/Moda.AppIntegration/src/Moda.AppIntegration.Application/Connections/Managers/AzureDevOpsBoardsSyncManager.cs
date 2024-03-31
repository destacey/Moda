using Ardalis.GuardClauses;
using MediatR;
using Moda.AppIntegration.Application.Connections.Queries;
using Moda.AppIntegration.Application.Interfaces;
using Moda.Common.Application.Requests.WorkManagement;
using Moda.Work.Application.WorkProcesses.Commands;
using Moda.Work.Application.WorkStatuses.Commands;
using Moda.Work.Application.WorkTypes.Commands;

namespace Moda.AppIntegration.Application.Connections.Managers;
public sealed class AzureDevOpsBoardsSyncManager(ILogger<AzureDevOpsBoardsSyncManager> logger, IAzureDevOpsService azureDevOpsService, ISender sender) : IAzureDevOpsBoardsSyncManager
{
    private readonly ILogger<AzureDevOpsBoardsSyncManager> _logger = logger;
    private readonly IAzureDevOpsService _azureDevOpsService = azureDevOpsService;
    private readonly ISender _sender = sender;

    public async Task<Result> Sync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Syncing Azure DevOps Boards");

        try
        {
            var connections = await _sender.Send(new GetConnectionsQuery(false, Connector.AzureDevOpsBoards), cancellationToken);
            if (!connections.Any(c => c.IsValidConfiguration && c.IsSyncEnabled))
            {
                var message = "No active Azure DevOps Boards connections found.";
                _logger.LogInformation(message);
                return Result.Failure(message);
            }

            var activeConnections = connections.Where(c => c.IsValidConfiguration && c.IsSyncEnabled).ToList();

            var activeConnectionsCount = activeConnections.Count;
            var activeWorkProcessesCount = 0;
            var activeWorkProcessesSyncedCount = 0;
            var activeWorkspacesCount = 0;
            var activeWorkspacesSyncedCount = 0;

            foreach (var connection in activeConnections)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    var message = "Cancellation requested. Stopping sync.";
                    _logger.LogInformation(message);
                    return Result.Success();
                }

                var connectionDetails = await _sender.Send(new GetAzureDevOpsBoardsConnectionQuery(connection.Id), cancellationToken);
                if (connectionDetails is null)
                {
                    _logger.LogError("Unable to retrieve connection details for Azure DevOps Boards connection with ID {ConnectionId}.", connection.Id);
                    continue;
                }

                var activeWorkProcesses = connectionDetails.Configuration.WorkProcesses
                    .Where(wp => wp.IntegrationState is not null && wp.IntegrationState.IsActive)
                    .ToList();

                if (activeWorkProcesses.Count == 0)
                {
                    _logger.LogInformation("No active work processes found for Azure DevOps Boards connection with ID {ConnectionId}.", connection.Id);
                    continue;
                }

                activeWorkProcessesCount += activeWorkProcesses.Count;
                foreach (var workProcess in activeWorkProcesses)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        var message = "Cancellation requested. Stopping sync.";
                        _logger.LogInformation(message);
                        return Result.Success();
                    }

                    var syncResult = await SyncWorkProcess(connectionDetails.Configuration.OrganizationUrl, connectionDetails.Configuration.PersonalAccessToken, workProcess.ExternalId, cancellationToken);
                    if (syncResult.IsFailure)
                    {
                        _logger.LogError("An error occurred while syncing Azure DevOps Boards work process {WorkProcessId}. Error: {Error}", workProcess.IntegrationState!.InternalId, syncResult.Error);
                        continue;
                    }

                    _logger.LogInformation("Successfully synced Azure DevOps Boards work process {WorkProcessId}.", workProcess.IntegrationState!.InternalId);
                    activeWorkProcessesSyncedCount++;

                    // TODO: sync workspaces
                    var activeWorkspaces = connectionDetails.Configuration.Workspaces
                        .Where(w => w.WorkProcessId == workProcess.ExternalId
                            && w.IntegrationState is not null
                            && w.IntegrationState.IsActive)
                        .ToList();

                    if (activeWorkspaces.Count == 0)
                    {
                        _logger.LogInformation("No active workspaces found for Azure DevOps Boards work process {WorkProcessId}.", workProcess.IntegrationState!.InternalId);
                        continue;
                    }

                    activeWorkspacesCount += activeWorkspaces.Count;
                    foreach (var workspace in activeWorkspaces)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            var message = "Cancellation requested. Stopping sync.";
                            _logger.LogInformation(message);
                            return Result.Success();
                        }

                        var syncWorkspaceResult = await SyncWorkspace(connectionDetails.Configuration.OrganizationUrl, connectionDetails.Configuration.PersonalAccessToken, workspace.ExternalId, cancellationToken);
                        if (syncWorkspaceResult.IsFailure)
                        {
                            _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId}. Error: {Error}", workspace.IntegrationState!.InternalId, syncWorkspaceResult.Error);
                            continue;
                        }

                        _logger.LogInformation("Successfully synced Azure DevOps Boards workspace {WorkspaceId}.", workspace.IntegrationState!.InternalId);
                        activeWorkspacesSyncedCount++;
                    }
                }
            }

            _logger.LogInformation("Synced {ActiveWorkProcessesSyncedCount} of {ActiveWorkProcessesCount} active work processes and {ActiveWorkspacesSyncedCount} of {ActiveWorkspacesCount} active workspaces for {ActiveConnectionsCount} active Azure DevOps Boards connections.", activeWorkProcessesSyncedCount, activeWorkProcessesCount, activeWorkspacesSyncedCount, activeWorkspacesCount, activeConnectionsCount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while trying to sync external employees.";
            _logger.LogError(ex, message);
            return Result.Failure(message);
        }
    }

    private async Task<Result> SyncWorkProcess(string organizationUrl, string personalAccessToken, Guid workProcessExternalId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workProcessExternalId, nameof(workProcessExternalId));

        // get the process, types, states, and workflow
        var processResult = await _azureDevOpsService.GetWorkProcess(organizationUrl, personalAccessToken, workProcessExternalId, cancellationToken);
        if (processResult.IsFailure)
            return processResult.ConvertFailure();

        // create new types
        if (processResult.Value.WorkTypes.Any())
        {
            var syncWorkTypesResult = await _sender.Send(new SyncExternalWorkTypesCommand(processResult.Value.WorkTypes), cancellationToken);
            if (syncWorkTypesResult.IsFailure)
                return syncWorkTypesResult.ConvertFailure<Guid>();
        }

        // create new statuses
        if (processResult.Value.WorkStatuses.Any())
        {
            var syncWorkStatusesResult = await _sender.Send(new SyncExternalWorkStatusesCommand(processResult.Value.WorkStatuses), cancellationToken);
            if (syncWorkStatusesResult.IsFailure)
                return syncWorkStatusesResult.ConvertFailure<Guid>();
        }

        // update the work process
        // TODO: update work process scheme
        var updateWorkProcessResult = await _sender.Send(new UpdateExternalWorkProcessCommand(processResult.Value), cancellationToken);

        return updateWorkProcessResult.IsSuccess
            ? Result.Success()
            : updateWorkProcessResult;
    }

    private async Task<Result> SyncWorkspace(string organizationUrl, string personalAccessToken, Guid workspaceExternalId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceExternalId, nameof(workspaceExternalId));

        var workspaceResult = await _azureDevOpsService.GetWorkspace(organizationUrl, personalAccessToken, workspaceExternalId, cancellationToken);
        if (workspaceResult.IsFailure)
            return workspaceResult.ConvertFailure();

        var updateResult = await _sender.Send(new UpdateExternalWorkspaceCommand(workspaceResult.Value), cancellationToken);

        return updateResult.IsSuccess
            ? Result.Success()
            : updateResult;
    }
}
