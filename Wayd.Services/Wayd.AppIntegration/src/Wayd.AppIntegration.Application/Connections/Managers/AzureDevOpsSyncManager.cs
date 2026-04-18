using Ardalis.GuardClauses;
using MediatR;
using Wayd.AppIntegration.Application.Connections.Dtos.AzureDevOps;
using Wayd.AppIntegration.Application.Connections.Queries;
using Wayd.AppIntegration.Application.Connections.Queries.AzureDevOps;
using Wayd.AppIntegration.Application.Interfaces;
using Wayd.AppIntegration.Application.Logging;
using Wayd.Common.Application.Enums;
using Wayd.Common.Application.Exceptions;
using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Logging;
using Wayd.Common.Application.Requests.Planning.Iterations;
using Wayd.Common.Application.Requests.WorkManagement.Commands;
using Wayd.Common.Application.Requests.WorkManagement.Dtos;
using Wayd.Common.Application.Requests.WorkManagement.Queries;
using Wayd.Common.Domain.Enums.AppIntegrations;
using Wayd.Common.Domain.Enums.Work;
using NodaTime;

namespace Wayd.AppIntegration.Application.Connections.Managers;

public sealed class AzureDevOpsSyncManager(ILogger<AzureDevOpsSyncManager> logger, IAzureDevOpsService azureDevOpsService, ISender sender, IAzureDevOpsInitManager initManager) : IAzureDevOpsSyncManager
{
    private readonly ILogger<AzureDevOpsSyncManager> _logger = logger;
    private readonly IAzureDevOpsService _azureDevOpsService = azureDevOpsService;
    private readonly ISender _sender = sender;
    private readonly IAzureDevOpsInitManager _initManager = initManager;

    private static readonly DateTime _minSyncDate = new(1900, 01, 01);

    // LoggerMessage delegates to avoid allocations for hot LogInformation calls
    private static readonly Action<ILogger, Exception?> _syncStarted = LoggerMessage.Define(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_SyncStarted.ToEventId(), "Syncing Azure DevOps");
    private static readonly Action<ILogger, Exception?> _cancellationRequested = LoggerMessage.Define(LogLevel.Information, AppEventId.AppIntegration_CancellationRequested.ToEventId(), "Cancellation requested. Stopping sync.");
    private static readonly Action<ILogger, Guid, Exception?> _workProcessSynced = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkProcessSynced.ToEventId(), "Successfully synced Azure DevOps work process {WorkProcessId}.");
    private static readonly Action<ILogger, Guid, Exception?> _workspaceSynced = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkspaceSynced.ToEventId(), "Successfully synced Azure DevOps workspace {WorkspaceId}.");
    private static readonly Action<ILogger, Guid, Exception?> _workspaceWorkItemsSynced = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkspaceWorkItemsSynced.ToEventId(), "Successfully synced Azure DevOps workspace {WorkspaceId} work items.");
    private static readonly Action<ILogger, Guid, Exception?> _noActiveWorkProcesses = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_NoActiveWorkProcesses.ToEventId(), "No active work processes found for Azure DevOps connection with ID {ConnectionId}.");
    private static readonly Action<ILogger, Guid, Exception?> _noActiveWorkspaces = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_NoActiveWorkspaces.ToEventId(), "No active workspaces found for Azure DevOps work process {WorkProcessId}.");
    private static readonly Action<ILogger, int, string, Exception?> _workItemsRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkItemsRetrieved.ToEventId(), "Retrieved {WorkItemCount} work items to sync for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, string, Exception?> _parentChangesRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_ParentChangesRetrieved.ToEventId(), "Retrieved {WorkItemParentChangesCount} work item parent changes to sync for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, string, Exception?> _dependencyChangesRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_DependencyChangesRetrieved.ToEventId(), "Retrieved {WorkItemDependencyChangesCount} work item dependency changes to sync for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, string, Exception?> _deletedWorkItemsRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_DeletedWorkItemsRetrieved.ToEventId(), "Retrieved {WorkItemCount} deleted work items for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, int, int, int, int, Exception?> _summary = LoggerMessage.Define<int, int, int, int, int>(LogLevel.Information, new EventId((int)AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_SyncSummary, nameof(AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_SyncSummary)), "Synced {ActiveWorkProcessesSyncedCount} of {ActiveWorkProcessesCount} active work processes and {ActiveWorkspacesSyncedCount} of {ActiveWorkspacesCount} active workspaces for {ActiveConnectionsCount} active Azure DevOps connections.");

    /// <summary>
    /// Tracks sync progress counters across connections.
    /// </summary>
    private sealed class ConnectionSyncResult
    {
        public int ActiveWorkProcesses { get; set; }
        public int WorkProcessesSynced { get; set; }
        public int ActiveWorkspaces { get; set; }
        public int WorkspacesSynced { get; set; }

        public void Add(ConnectionSyncResult other)
        {
            ActiveWorkProcesses += other.ActiveWorkProcesses;
            WorkProcessesSynced += other.WorkProcessesSynced;
            ActiveWorkspaces += other.ActiveWorkspaces;
            WorkspacesSynced += other.WorkspacesSynced;
        }
    }

    /// <summary>
    /// Bundles the connection-level configuration that is threaded through every sync step.
    /// Validated at construction so callers don't need to re-check.
    /// </summary>
    private sealed record SyncContext
    {
        public string OrganizationUrl { get; }
        public string PersonalAccessToken { get; }
        public string SystemId { get; }
        public Guid SyncId { get; }

        public SyncContext(string organizationUrl, string personalAccessToken, string systemId, Guid syncId)
        {
            OrganizationUrl = Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
            PersonalAccessToken = Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
            SystemId = Guard.Against.NullOrWhiteSpace(systemId, nameof(systemId));
            SyncId = Guard.Against.Default(syncId, nameof(syncId));
        }
    }

    public async Task<Result> Sync(SyncType syncType, CancellationToken cancellationToken)
    {
        _syncStarted(_logger, null);

        var syncId = Guid.CreateVersion7();
        using (_logger.BeginScope(new Dictionary<string, object> { ["SyncId"] = syncId }))
        {
            try
            {
                var connections = await _sender.Send(new GetConnectionsQuery(false, Connector.AzureDevOps), cancellationToken);
                var activeConnections = connections.Where(c => c.IsValidConfiguration && c.IsSyncEnabled == true).ToList();
                if (activeConnections.Count == 0)
                {
                    var message = "No active Azure DevOps connections found.";
                    _logger.LogInformation(message);
                    return Result.Failure(message);
                }

                var totals = new ConnectionSyncResult();

                foreach (var connection in activeConnections)
                {
                    using (_logger.BeginScope(new Dictionary<string, object> { ["ConnectionId"] = connection.Id }))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _cancellationRequested(_logger, null);
                            return Result.Success();
                        }

                        var connectionResult = await SyncConnection(
                            connection, syncType, syncId, cancellationToken);

                        if (connectionResult.IsSuccess)
                            totals.Add(connectionResult.Value);
                    }
                }

                _summary(_logger, totals.WorkProcessesSynced, totals.ActiveWorkProcesses, totals.WorkspacesSynced, totals.ActiveWorkspaces, activeConnections.Count, null);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while trying to sync Azure DevOps.");
                throw;
            }
        }
    }

    /// <summary>
    /// Syncs a single connection: organization config, work processes, workspaces, and dependencies.
    /// </summary>
    private async Task<Result<ConnectionSyncResult>> SyncConnection(
        ConnectionListDto connection, SyncType syncType, Guid syncId, CancellationToken cancellationToken)
    {
        var syncOrganizationResult = await _initManager.SyncOrganizationConfiguration(connection.Id, cancellationToken, syncId);
        if (syncOrganizationResult.IsFailure)
        {
            _logger.LogError("An error occurred while syncing Azure DevOps organization configuration for connection with ID {ConnectionId}. Error: {Error}", connection.Id, syncOrganizationResult.Error);
            return Result.Failure<ConnectionSyncResult>(syncOrganizationResult.Error);
        }

        var connectionDetails = await _sender.Send(new GetAzureDevOpsConnectionQuery(connection.Id), cancellationToken);
        if (connectionDetails is null)
        {
            _logger.LogError("Unable to retrieve connection details for Azure DevOps connection with ID {ConnectionId}.", connection.Id);
            return Result.Failure<ConnectionSyncResult>("Unable to retrieve connection details.");
        }

        var configuration = connectionDetails.Configuration;
        var teamConfiguration = connectionDetails.TeamConfiguration;

        var ctx = new SyncContext(
            configuration.OrganizationUrl,
            configuration.PersonalAccessToken,
            connectionDetails.SystemId!,
            syncId);

        // Build a lookup for workspace teams to avoid re-enumerating the full collection per workspace
        var workspaceTeamsLookup = teamConfiguration?.WorkspaceTeams is not null
            ? teamConfiguration.WorkspaceTeams.GroupBy(t => t.WorkspaceId).ToDictionary(g => g.Key, g => g.ToArray())
            : [];

        var activeWorkProcesses = configuration.WorkProcesses
            .Where(wp => wp.IntegrationState is not null && wp.IntegrationState.IsActive)
            .ToList();

        if (activeWorkProcesses.Count == 0)
        {
            _noActiveWorkProcesses(_logger, connection.Id, null);
            return Result.Success(new ConnectionSyncResult());
        }

        var result = new ConnectionSyncResult { ActiveWorkProcesses = activeWorkProcesses.Count };

        foreach (var workProcess in activeWorkProcesses)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _cancellationRequested(_logger, null);
                return Result.Success(result);
            }

            var syncResult = await SyncWorkProcess(ctx, workProcess.ExternalId, workProcess.IntegrationState!.InternalId, cancellationToken);
            if (syncResult.IsFailure)
            {
                _logger.LogError("An error occurred while syncing Azure DevOps work process {WorkProcessId}. Error: {Error}", workProcess.IntegrationState!.InternalId, syncResult.Error);
                continue;
            }

            _workProcessSynced(_logger, workProcess.IntegrationState!.InternalId, null);
            result.WorkProcessesSynced++;

            var activeWorkspaces = configuration.Workspaces
                .Where(w => w.WorkProcessId == workProcess.ExternalId
                    && w.IntegrationState is not null
                    && w.IntegrationState.IsActive)
                .ToList();

            if (activeWorkspaces.Count == 0)
            {
                _noActiveWorkspaces(_logger, workProcess.IntegrationState!.InternalId, null);
                continue;
            }

            result.ActiveWorkspaces += activeWorkspaces.Count;
            foreach (var workspace in activeWorkspaces)
            {
                using (_logger.BeginScope(new Dictionary<string, object> { ["WorkspaceId"] = workspace.IntegrationState!.InternalId }))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _cancellationRequested(_logger, null);
                        return Result.Success(result);
                    }

                    var workspaceTeams = workspaceTeamsLookup.TryGetValue(workspace.ExternalId, out var wt) ? wt : [];

                    var syncWorkspaceResult = await SyncWorkspaceData(ctx, syncType, workspace, workspaceTeams, cancellationToken);
                    if (syncWorkspaceResult.IsSuccess)
                        result.WorkspacesSynced++;
                }
            }
        }

        var processDependenciesResult = await _sender.Send(new ProcessDependenciesCommand(ctx.SystemId), cancellationToken);
        if (processDependenciesResult.IsFailure)
        {
            _logger.LogError("An error occurred while processing dependencies for Azure DevOps connection {ConnectionId}. Error: {Error}", connection.Id, processDependenciesResult.Error);
        }

        return Result.Success(result);
    }

    /// <summary>
    /// Syncs a single workspace: configuration, iterations, work items, parent changes, dependency changes, and deleted items.
    /// Workspace config and iteration sync failures are blocking — subsequent steps depend on them.
    /// Work item sync steps (items, parent changes, dependency changes, deleted items) are attempted independently.
    /// </summary>
    private async Task<Result> SyncWorkspaceData(SyncContext ctx, SyncType syncType, AzureDevOpsWorkspaceDto workspace, AzureDevOpsWorkspaceTeamDto[] workspaceTeams, CancellationToken cancellationToken)
    {
        var workspaceId = workspace.IntegrationState!.InternalId;

        var syncWorkspaceResult = await SyncWorkspace(ctx, workspace.ExternalId, cancellationToken);
        if (syncWorkspaceResult.IsFailure)
        {
            _logger.LogError("An error occurred while syncing Azure DevOps workspace {WorkspaceId}. Error: {Error}", workspaceId, syncWorkspaceResult.Error);
            return syncWorkspaceResult;
        }

        _workspaceSynced(_logger, workspaceId, null);

        BuildTeamSettingsAndMappings(workspaceTeams, out var teamSettings, out var teamMappings);

        var syncIterationsResult = await SyncIterations(ctx, workspace.Name, teamSettings, teamMappings, cancellationToken);
        if (syncIterationsResult.IsFailure)
        {
            _logger.LogError("An error occurred while syncing Azure DevOps workspace {WorkspaceId} iterations. Error: {Error}", workspaceId, syncIterationsResult.Error);
            return syncIterationsResult;
        }

        var workTypesResult = await _sender.Send(new GetWorkspaceWorkTypesQuery(workspaceId), cancellationToken);
        if (workTypesResult.IsFailure)
            return workTypesResult.ConvertFailure();

        var workTypeDtos = workTypesResult.Value;
        var workTypeNames = new string[workTypeDtos.Count];
        for (int i = 0; i < workTypeDtos.Count; i++)
            workTypeNames[i] = workTypeDtos[i].Name;

        try
        {
            var lastChangedDate = syncType switch
            {
                SyncType.Full => _minSyncDate,
                SyncType.Differential => await GetWorkspaceMostRecentChangeDate(_sender, workspaceId, cancellationToken),
                _ => _minSyncDate
            };

            // Each work item sync step is attempted independently
            var hasWorkItemSyncError = false;

            var syncWorkItemsResult = await SyncWorkItems(ctx, lastChangedDate, workspaceId, workspace.Name, teamSettings, teamMappings, workTypeNames, cancellationToken);
            if (syncWorkItemsResult.IsFailure)
            {
                _logger.LogError("An error occurred while syncing Azure DevOps workspace {WorkspaceId} work items. Error: {Error}", workspaceId, syncWorkItemsResult.Error);
                hasWorkItemSyncError = true;
            }

            if (syncType == SyncType.Differential)
            {
                var syncWorkItemParentChangesResult = await SyncWorkItemParentChanges(ctx, lastChangedDate, workspaceId, workspace.Name, workTypeNames, cancellationToken);
                if (syncWorkItemParentChangesResult.IsFailure)
                {
                    _logger.LogError("An error occurred while syncing Azure DevOps workspace {WorkspaceId} work item parent changes. Error: {Error}", workspaceId, syncWorkItemParentChangesResult.Error);
                    hasWorkItemSyncError = true;
                }
            }

            var syncWorkItemDependencyChangesResult = await SyncWorkItemDependencyChanges(ctx, lastChangedDate, workspaceId, workspace.Name, workTypeNames, cancellationToken);
            if (syncWorkItemDependencyChangesResult.IsFailure)
            {
                _logger.LogError("An error occurred while syncing Azure DevOps workspace {WorkspaceId} work item dependency changes. Error: {Error}", workspaceId, syncWorkItemDependencyChangesResult.Error);
                hasWorkItemSyncError = true;
            }

            var syncDeletedWorkItemsResult = await SyncDeletedWorkItems(ctx, workspaceId, workspace.Name, lastChangedDate, cancellationToken);
            if (syncDeletedWorkItemsResult.IsFailure)
            {
                _logger.LogError("An error occurred while syncing Azure DevOps workspace {WorkspaceId} deleted work items. Error: {Error}", workspaceId, syncDeletedWorkItemsResult.Error);
                hasWorkItemSyncError = true;
            }

            if (!hasWorkItemSyncError)
                _workspaceWorkItemsSynced(_logger, workspaceId, null);

            return Result.Success();
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "A validation exception occurred while syncing Azure DevOps workspace {WorkspaceId} work items.", workspaceId);
            return Result.Failure(ex.Message);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Sync operation was canceled while syncing Azure DevOps workspace {WorkspaceId} work items.", workspaceId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while syncing Azure DevOps workspace {WorkspaceId} work items.", workspaceId);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> SyncWorkProcess(SyncContext ctx, Guid workProcessExternalId, Guid workProcessId, CancellationToken cancellationToken)
    {
        Guard.Against.Default(workProcessExternalId, nameof(workProcessExternalId));
        Guard.Against.Default(workProcessId, nameof(workProcessId));

        // get the process, types, states, and workflow
        var processResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetWorkProcess", () => _azureDevOpsService.GetWorkProcess(ctx.OrganizationUrl, ctx.PersonalAccessToken, workProcessExternalId, cancellationToken), ctx.SyncId);
        if (processResult.IsFailure)
            return processResult.ConvertFailure();

        // create new types
        var workTypes = processResult.Value.WorkTypes.OfType<IExternalWorkType>().ToList();
        if (workTypes.Count != 0)
        {
            var levels = await _sender.Send(new GetWorkTypeLevelsQuery(), cancellationToken);
            if (levels is null)
                return Result.Failure("Unable to get work type levels.");

            int defaultLevelId = -1;
            foreach (var l in levels)
            {
                if (l.Tier.Id == (int)WorkTypeTier.Other)
                {
                    defaultLevelId = l.Id;
                    break;
                }
            }

            if (defaultLevelId == -1)
                return Result.Failure("Unable to get work type levels.");

            var syncWorkTypesResult = await _sender.Send(new SyncExternalWorkTypesCommand(workTypes, defaultLevelId), cancellationToken);
            if (syncWorkTypesResult.IsFailure)
                return syncWorkTypesResult;
        }

        // create new statuses
        if (processResult.Value.WorkStatuses.Count != 0)
        {
            var syncWorkStatusesResult = await _sender.Send(new SyncExternalWorkStatusesCommand(processResult.Value.WorkStatuses), cancellationToken);
            if (syncWorkStatusesResult.IsFailure)
                return syncWorkStatusesResult;
        }

        // get the work process scheme, work type, and workflow
        var workProcessSchemes = await _sender.Send(new GetWorkProcessSchemesQuery(workProcessId), cancellationToken);

        // create or update work flows
        var workProcessSchemesByWorkTypeName = workProcessSchemes.ToDictionary(s => s.WorkType.Name);

        var workflowMappings = new List<CreateWorkProcessSchemeDto>(processResult.Value.WorkTypes.Count);
        foreach (var workType in processResult.Value.WorkTypes)
        {
            workProcessSchemesByWorkTypeName.TryGetValue(workType.Name, out var scheme);
            if (scheme is null || scheme.Workflow is null)
            {
                // create new workflow
                var createWorkflowResult = await _sender.Send(new CreateExternalWorkflowCommand(
                $"{processResult.Value.Name} - {workType.Name}",
                "Auto-generated workflow for Azure DevOps work process.",
                workType), cancellationToken);
                if (createWorkflowResult.IsFailure)
                    return createWorkflowResult.ConvertFailure();

                workflowMappings.Add(CreateWorkProcessSchemeDto.Create(workType.Name, workType.IsActive, createWorkflowResult.Value));
            }
            else
            {
                var syncWorkflowResult = await _sender.Send(new UpdateExternalWorkflowCommand(scheme.Workflow.Id, scheme.Workflow.Name, scheme.Workflow.Description, workType), cancellationToken);
                if (syncWorkflowResult.IsFailure)
                    return syncWorkflowResult;

                workflowMappings.Add(CreateWorkProcessSchemeDto.Create(workType.Name, workType.IsActive, scheme.Workflow.Id));
            }
        }

        // update the work process
        var updateWorkProcessResult = await _sender.Send(new UpdateExternalWorkProcessCommand(processResult.Value, processResult.Value.WorkTypes, workflowMappings), cancellationToken);

        return updateWorkProcessResult.IsSuccess
            ? Result.Success()
            : updateWorkProcessResult;
    }

    private async Task<Result> SyncWorkspace(SyncContext ctx, Guid workspaceExternalId, CancellationToken cancellationToken)
    {
        Guard.Against.Default(workspaceExternalId, nameof(workspaceExternalId));

        var workspaceResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetWorkspace", () => _azureDevOpsService.GetWorkspace(ctx.OrganizationUrl, ctx.PersonalAccessToken, workspaceExternalId, cancellationToken), ctx.SyncId);
        if (workspaceResult.IsFailure)
            return workspaceResult.ConvertFailure();

        var updateResult = await _sender.Send(new UpdateExternalWorkspaceCommand(workspaceResult.Value), cancellationToken);

        return updateResult.IsSuccess
            ? Result.Success()
            : updateResult;
    }

    private async Task<Result> SyncIterations(SyncContext ctx, string azdoWorkspaceName, Dictionary<Guid, Guid?> teamSettings, Dictionary<Guid, Guid?> teamMappings, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(azdoWorkspaceName, nameof(azdoWorkspaceName));

        var iterationsResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetIterations", () => _azureDevOpsService.GetIterations(ctx.OrganizationUrl, ctx.PersonalAccessToken, azdoWorkspaceName, teamSettings, cancellationToken), ctx.SyncId);
        if (iterationsResult.IsFailure)
            return iterationsResult.ConvertFailure();

        var syncResult = await _sender.Send(new SyncAzureDevOpsIterationsCommand(ctx.SystemId, iterationsResult.Value, teamMappings), cancellationToken);

        return syncResult;
    }

    private async Task<Result> SyncWorkItems(SyncContext ctx, DateTime lastChangedDate, Guid workspaceId, string azdoWorkspaceName, Dictionary<Guid, Guid?> teamSettings, Dictionary<Guid, Guid?> teamMappings, string[] workTypeNames, CancellationToken cancellationToken)
    {
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var workItemsResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetWorkItems", () => _azureDevOpsService.GetWorkItems(ctx.OrganizationUrl, ctx.PersonalAccessToken, azdoWorkspaceName, lastChangedDate, workTypeNames, teamSettings, cancellationToken), ctx.SyncId);
        if (workItemsResult.IsFailure)
            return workItemsResult.ConvertFailure();

        _workItemsRetrieved(_logger, workItemsResult.Value.Count, azdoWorkspaceName, null);

        var iterationMappings = await _sender.Send(new GetIterationMappingsQuery(Connector.AzureDevOps, ctx.SystemId), cancellationToken);

        return workItemsResult.Value.Count == 0
            ? Result.Success()
            : await _sender.Send(new SyncExternalWorkItemsCommand(workspaceId, workItemsResult.Value, teamMappings, iterationMappings), cancellationToken);
    }

    private async Task<Result> SyncWorkItemParentChanges(SyncContext ctx, DateTime lastChangedDate, Guid workspaceId, string azdoWorkspaceName, string[] workTypeNames, CancellationToken cancellationToken)
    {
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var parentLinkChangesResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetParentLinkChanges", () => _azureDevOpsService.GetParentLinkChanges(ctx.OrganizationUrl, ctx.PersonalAccessToken, azdoWorkspaceName, lastChangedDate, workTypeNames, cancellationToken), ctx.SyncId);
        if (parentLinkChangesResult.IsFailure)
            return parentLinkChangesResult.ConvertFailure();

        _parentChangesRetrieved(_logger, parentLinkChangesResult.Value.Count, azdoWorkspaceName, null);

        return parentLinkChangesResult.Value.Count == 0
            ? Result.Success()
            : await _sender.Send(new SyncExternalWorkItemParentChangesCommand(workspaceId, parentLinkChangesResult.Value), cancellationToken);
    }

    private async Task<Result> SyncWorkItemDependencyChanges(SyncContext ctx, DateTime lastChangedDate, Guid workspaceId, string azdoWorkspaceName, string[] workTypeNames, CancellationToken cancellationToken)
    {
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var dependencyLinkChangesResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetDependencyLinkChanges", () => _azureDevOpsService.GetDependencyLinkChanges(ctx.OrganizationUrl, ctx.PersonalAccessToken, azdoWorkspaceName, lastChangedDate, workTypeNames, cancellationToken), ctx.SyncId);
        if (dependencyLinkChangesResult.IsFailure)
            return dependencyLinkChangesResult.ConvertFailure();

        _dependencyChangesRetrieved(_logger, dependencyLinkChangesResult.Value.Count, azdoWorkspaceName, null);

        return dependencyLinkChangesResult.Value.Count == 0
            ? Result.Success()
            : await _sender.Send(new SyncExternalWorkItemDependencyChangesCommand(workspaceId, dependencyLinkChangesResult.Value), cancellationToken);
    }

    private async Task<Result> SyncDeletedWorkItems(SyncContext ctx, Guid workspaceId, string azdoWorkspaceName, DateTime lastChangedDate, CancellationToken cancellationToken)
    {
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var getDeletedWorkItemIdsResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetDeletedWorkItemIds", () => _azureDevOpsService.GetDeletedWorkItemIds(ctx.OrganizationUrl, ctx.PersonalAccessToken, azdoWorkspaceName, lastChangedDate, cancellationToken), ctx.SyncId);
        if (getDeletedWorkItemIdsResult.IsFailure)
            return getDeletedWorkItemIdsResult.ConvertFailure();

        _deletedWorkItemsRetrieved(_logger, getDeletedWorkItemIdsResult.Value.Length, azdoWorkspaceName, null);

        return getDeletedWorkItemIdsResult.Value.Length == 0
            ? Result.Success()
            : await _sender.Send(new DeleteExternalWorkItemsCommand(workspaceId, getDeletedWorkItemIdsResult.Value), cancellationToken);
    }

    private static void BuildTeamSettingsAndMappings(AzureDevOpsWorkspaceTeamDto[] workspaceTeams, out Dictionary<Guid, Guid?> teamSettings, out Dictionary<Guid, Guid?> teamMappings)
    {
        if (workspaceTeams == null || workspaceTeams.Length == 0)
        {
            teamSettings = [];
            teamMappings = [];
            return;
        }

        teamSettings = new (workspaceTeams.Length);
        teamMappings = new(workspaceTeams.Length);

        foreach (var team in workspaceTeams)
        {
            if (team.InternalTeamId is null)
                continue;

            teamSettings[team.TeamId] = team.BoardId;
            teamMappings[team.TeamId] = team.InternalTeamId;
        }
    }

    private static async Task<DateTime> GetWorkspaceMostRecentChangeDate(ISender sender, Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceMostRecentChangeDateQuery(workspaceId), cancellationToken);
        return result.IsSuccess && result.Value != null
            ? ((Instant)result.Value).ToDateTimeUtc()
            : _minSyncDate;
    }
}
