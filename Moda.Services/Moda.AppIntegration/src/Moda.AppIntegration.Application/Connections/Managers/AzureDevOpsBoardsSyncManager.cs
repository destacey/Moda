﻿using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Moda.AppIntegration.Application.Connections.Queries;
using Moda.AppIntegration.Application.Interfaces;
using Moda.Common.Application.Enums;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Requests.WorkManagement;
using Moda.Work.Application.WorkProcesses.Commands;
using Moda.Work.Application.WorkStatuses.Commands;
using Moda.Work.Application.WorkTypes.Commands;
using Moda.Work.Domain.Models;
using NodaTime;

namespace Moda.AppIntegration.Application.Connections.Managers;

public sealed class AzureDevOpsBoardsSyncManager(ILogger<AzureDevOpsBoardsSyncManager> logger, IAzureDevOpsService azureDevOpsService, ISender sender) : IAzureDevOpsBoardsSyncManager
{
    private readonly ILogger<AzureDevOpsBoardsSyncManager> _logger = logger;
    private readonly IAzureDevOpsService _azureDevOpsService = azureDevOpsService;
    private readonly ISender _sender = sender;

    public async Task<Result> Sync(SyncType syncType, CancellationToken cancellationToken)
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

            // TODO: convert to a sync result object that can be returned to hangfire
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

                        try
                        {
                            var syncWorkItemsResult = await SyncWorkItems(connectionDetails.Configuration.OrganizationUrl, connectionDetails.Configuration.PersonalAccessToken, syncType, workspace.IntegrationState!.InternalId, workspace.Name, cancellationToken);
                            if (syncWorkItemsResult.IsFailure)
                            {
                                _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId} work items. Error: {Error}", workspace.IntegrationState!.InternalId, syncWorkItemsResult.Error);
                                continue;
                            }

                            var syncDeletedWorkItemsResult = await SyncDeletedWorkItems(connectionDetails.Configuration.OrganizationUrl, connectionDetails.Configuration.PersonalAccessToken, workspace.IntegrationState!.InternalId, workspace.Name, cancellationToken);
                            if (syncDeletedWorkItemsResult.IsFailure)
                            {
                                _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId} deleted work items. Error: {Error}", workspace.IntegrationState!.InternalId, syncDeletedWorkItemsResult.Error);
                                continue;
                            }

                            _logger.LogInformation("Successfully synced Azure DevOps Boards workspace {WorkspaceId} work items.", workspace.IntegrationState!.InternalId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An exception occurred while syncing Azure DevOps Boards workspace {WorkspaceId} work items.", workspace.IntegrationState!.InternalId);
                            continue;
                        }
                    }
                }
            }

            _logger.LogInformation("Synced {ActiveWorkProcessesSyncedCount} of {ActiveWorkProcessesCount} active work processes and {ActiveWorkspacesSyncedCount} of {ActiveWorkspacesCount} active workspaces for {ActiveConnectionsCount} active Azure DevOps Boards connections.", activeWorkProcessesSyncedCount, activeWorkProcessesCount, activeWorkspacesSyncedCount, activeWorkspacesCount, activeConnectionsCount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            string message = "An exception occurred while trying to sync Azure DevOps Boards.";
            _logger.LogError(ex, message);
            throw;
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
        var workTypes = processResult.Value.WorkTypes.OfType<IExternalWorkType>().ToList();
        if (workTypes.Count != 0)
        {
            var syncWorkTypesResult = await _sender.Send(new SyncExternalWorkTypesCommand(workTypes), cancellationToken);
            if (syncWorkTypesResult.IsFailure)
                return syncWorkTypesResult.ConvertFailure<Guid>();
        }

        // create new statuses
        if (processResult.Value.WorkStatuses.Count != 0)
        {
            var syncWorkStatusesResult = await _sender.Send(new SyncExternalWorkStatusesCommand(processResult.Value.WorkStatuses), cancellationToken);
            if (syncWorkStatusesResult.IsFailure)
                return syncWorkStatusesResult.ConvertFailure<Guid>();
        }

        // update the work process
        // TODO: update work process scheme
        var updateWorkProcessResult = await _sender.Send(new UpdateExternalWorkProcessCommand(processResult.Value, processResult.Value.WorkTypes), cancellationToken);

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

    private async Task<Result> SyncWorkItems(string organizationUrl, string personalAccessToken, SyncType syncType, Guid workspaceId, string azdoWorkspaceName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var workTypesResult = await _sender.Send(new GetWorkspaceWorkTypesQuery(workspaceId), cancellationToken);
        if (workTypesResult.IsFailure)
            return workTypesResult.ConvertFailure();

        var lastChangedDate = syncType switch
        {
            SyncType.Full => new DateTime(1900, 01, 01),
            SyncType.Differential => await GetWorkspaceMostRecentChangeDate(_sender, workspaceId, cancellationToken),
            _ => new DateTime(1900, 01, 01)
        };        

        var workItemsResult = await _azureDevOpsService.GetWorkItems(organizationUrl, personalAccessToken, azdoWorkspaceName, lastChangedDate, workTypesResult.Value.Select(t => t.Name).ToArray(), cancellationToken);
        if (workItemsResult.IsFailure)
            return workItemsResult.ConvertFailure();

        _logger.LogInformation("Retrieved {WorkItemCount} work items to sync for Azure DevOps project {Project}.", workItemsResult.Value.Count, azdoWorkspaceName);

        if (workItemsResult.Value.Count == 0)
            return Result.Success();

        var syncWorkItemsResult = await _sender.Send(new SyncExternalWorkItemsCommand(workspaceId, workItemsResult.Value), cancellationToken);

        return syncWorkItemsResult.IsSuccess
            ? Result.Success()
            : syncWorkItemsResult;
        
        static async Task<DateTime> GetWorkspaceMostRecentChangeDate(ISender sender, Guid workspaceId, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWorkspaceMostRecentChangeDateQuery(workspaceId), cancellationToken);
            return result.IsSuccess && result.Value != null
                ? ((Instant)result.Value).ToDateTimeUtc()
                : new DateTime(1900, 01, 01);
        }
    }

    private async Task<Result> SyncDeletedWorkItems(string organizationUrl, string personalAccessToken, Guid workspaceId, string azdoWorkspaceName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var getDeletedWorkItemIdsResult = await _azureDevOpsService.GetDeletedWorkItemIds(organizationUrl, personalAccessToken, azdoWorkspaceName, cancellationToken);
        if (getDeletedWorkItemIdsResult.IsFailure)
            return getDeletedWorkItemIdsResult.ConvertFailure();

        int deletedWorkItemCount = getDeletedWorkItemIdsResult.Value.Length;
        _logger.LogInformation("Retrieved {WorkItemCount} deleted work items for Azure DevOps project {Project}.", deletedWorkItemCount, azdoWorkspaceName);

        if (deletedWorkItemCount > 0)
        {
            var deleteWorkItemsResult = await _sender.Send(new DeleteExternalWorkItemsCommand(workspaceId, getDeletedWorkItemIdsResult.Value), cancellationToken);
            if (deleteWorkItemsResult.IsFailure)
                return deleteWorkItemsResult;
        }

        return Result.Success();
    }
}
