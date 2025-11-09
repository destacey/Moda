using Ardalis.GuardClauses;
using MediatR;
using Moda.AppIntegration.Application.Connections.Queries;
using Moda.AppIntegration.Application.Interfaces;
using Moda.AppIntegration.Application.Logging;
using Moda.Common.Application.Enums;
using Moda.Common.Application.Exceptions;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Logging;
using Moda.Common.Application.Requests.Planning.Iterations;
using Moda.Common.Application.Requests.WorkManagement;
using Moda.Common.Domain.Enums.AppIntegrations;
using Moda.Common.Domain.Enums.Work;
using Moda.Work.Application.Workflows.Dtos;
using Moda.Work.Application.WorkStatuses.Commands;
using Moda.Work.Application.WorkTypes.Commands;
using NodaTime;

namespace Moda.AppIntegration.Application.Connections.Managers;

public sealed class AzureDevOpsBoardsSyncManager(ILogger<AzureDevOpsBoardsSyncManager> logger, IAzureDevOpsService azureDevOpsService, ISender sender, IAzureDevOpsBoardsInitManager initManager) : IAzureDevOpsBoardsSyncManager
{
    private readonly ILogger<AzureDevOpsBoardsSyncManager> _logger = logger;
    private readonly IAzureDevOpsService _azureDevOpsService = azureDevOpsService;
    private readonly ISender _sender = sender;
    private readonly IAzureDevOpsBoardsInitManager _initManager = initManager;

    private static readonly DateTime _minSyncDate = new(1900, 01, 01);

    // LoggerMessage delegates to avoid allocations for hot LogInformation calls
    private static readonly Action<ILogger, Exception?> _syncStarted = LoggerMessage.Define(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_SyncStarted.ToEventId(), "Syncing Azure DevOps Boards");
    private static readonly Action<ILogger, Exception?> _cancellationRequested = LoggerMessage.Define(LogLevel.Information, AppEventId.AppIntegration_CancellationRequested.ToEventId(), "Cancellation requested. Stopping sync.");
    private static readonly Action<ILogger, Guid, Exception?> _workProcessSynced = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkProcessSynced.ToEventId(), "Successfully synced Azure DevOps Boards work process {WorkProcessId}.");
    private static readonly Action<ILogger, Guid, Exception?> _workspaceSynced = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkspaceSynced.ToEventId(), "Successfully synced Azure DevOps Boards workspace {WorkspaceId}.");
    private static readonly Action<ILogger, Guid, Exception?> _workspaceWorkItemsSynced = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkspaceWorkItemsSynced.ToEventId(), "Successfully synced Azure DevOps Boards workspace {WorkspaceId} work items.");
    private static readonly Action<ILogger, Guid, Exception?> _noActiveWorkProcesses = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_NoActiveWorkProcesses.ToEventId(), "No active work processes found for Azure DevOps Boards connection with ID {ConnectionId}.");
    private static readonly Action<ILogger, Guid, Exception?> _noActiveWorkspaces = LoggerMessage.Define<Guid>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_NoActiveWorkspaces.ToEventId(), "No active workspaces found for Azure DevOps Boards work process {WorkProcessId}.");
    private static readonly Action<ILogger, int, string, Exception?> _workItemsRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_WorkItemsRetrieved.ToEventId(), "Retrieved {WorkItemCount} work items to sync for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, string, Exception?> _parentChangesRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_ParentChangesRetrieved.ToEventId(), "Retrieved {WorkItemParentChangesCount} work item parent changes to sync for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, string, Exception?> _dependencyChangesRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_DependencyChangesRetrieved.ToEventId(), "Retrieved {WorkItemDependencyChangesCount} work item dependency changes to sync for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, string, Exception?> _deletedWorkItemsRetrieved = LoggerMessage.Define<int, string>(LogLevel.Information, AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_DeletedWorkItemsRetrieved.ToEventId(), "Retrieved {WorkItemCount} deleted work items for Azure DevOps project {Project}.");
    private static readonly Action<ILogger, int, int, int, int, int, Exception?> _summary = LoggerMessage.Define<int, int, int, int, int>(LogLevel.Information, new EventId((int)AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_SyncSummary, nameof(AppEventId.AppIntegration_AzureDevOpsBoardsSyncManager_SyncSummary)), "Synced {ActiveWorkProcessesSyncedCount} of {ActiveWorkProcessesCount} active work processes and {ActiveWorkspacesSyncedCount} of {ActiveWorkspacesCount} active workspaces for {ActiveConnectionsCount} active Azure DevOps Boards connections.");

    public async Task<Result> Sync(SyncType syncType, CancellationToken cancellationToken)
    {
        _syncStarted(_logger, null);

        var syncId = Guid.NewGuid();
        using (_logger.BeginScope(new Dictionary<string, object> { ["SyncId"] = syncId }))
        {
            try
            {
                var connections = await _sender.Send(new GetConnectionsQuery(false, Connector.AzureDevOps), cancellationToken);
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
                    using (_logger.BeginScope(new Dictionary<string, object> { ["ConnectionId"] = connection.Id }))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _cancellationRequested(_logger, null);
                            return Result.Success();
                        }

                        // Do we need to sync the organization configuration or do we just need to sync teams?
                        var syncOrganizationResult = await _initManager.SyncOrganizationConfiguration(connection.Id, cancellationToken);
                        if (syncOrganizationResult.IsFailure)
                        {
                            _logger.LogError("An error occurred while syncing Azure DevOps Boards organization configuration for connection with ID {ConnectionId}. Error: {Error}", connection.Id, syncOrganizationResult.Error);
                            continue;
                        }

                        var connectionDetails = await _sender.Send(new GetAzureDevOpsBoardsConnectionQuery(connection.Id), cancellationToken);
                        if (connectionDetails is null)
                        {
                            _logger.LogError("Unable to retrieve connection details for Azure DevOps Boards connection with ID {ConnectionId}.", connection.Id);
                            continue;
                        }

                        var configuration = connectionDetails.Configuration;
                        var teamConfiguration = connectionDetails.TeamConfiguration;

                        // Build a lookup for workspace teams to avoid re-enumerating the full collection per workspace
                        var workspaceTeamsLookup = teamConfiguration?.WorkspaceTeams is not null
                            ? teamConfiguration.WorkspaceTeams.GroupBy(t => t.WorkspaceId).ToDictionary(g => g.Key, g => g.ToArray())
                            : new Dictionary<Guid, AzureDevOpsBoardsWorkspaceTeamDto[]>();

                        var activeWorkProcesses = configuration.WorkProcesses
                        .Where(wp => wp.IntegrationState is not null && wp.IntegrationState.IsActive)
                        .ToList();

                        if (activeWorkProcesses.Count == 0)
                        {
                            _noActiveWorkProcesses(_logger, connection.Id, null);
                            continue;
                        }

                        activeWorkProcessesCount += activeWorkProcesses.Count;
                        foreach (var workProcess in activeWorkProcesses)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                _cancellationRequested(_logger, null);
                                return Result.Success();
                            }

                            var syncResult = await SyncWorkProcess(configuration.OrganizationUrl, configuration.PersonalAccessToken, workProcess.ExternalId, workProcess.IntegrationState!.InternalId, syncId, cancellationToken);
                            if (syncResult.IsFailure)
                            {
                                _logger.LogError("An error occurred while syncing Azure DevOps Boards work process {WorkProcessId}. Error: {Error}", workProcess.IntegrationState!.InternalId, syncResult.Error);
                                continue;
                            }

                            _workProcessSynced(_logger, workProcess.IntegrationState!.InternalId, null);

                            activeWorkProcessesSyncedCount++;

                            // TODO: sync workspaces
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

                            activeWorkspacesCount += activeWorkspaces.Count;
                            foreach (var workspace in activeWorkspaces)
                            {
                                using (_logger.BeginScope(new Dictionary<string, object> { ["WorkspaceId"] = workspace.IntegrationState!.InternalId }))
                                {
                                    if (cancellationToken.IsCancellationRequested)
                                    {
                                        _cancellationRequested(_logger, null);
                                        return Result.Success();
                                    }

                                    var workspaceId = workspace.IntegrationState!.InternalId;

                                    var syncWorkspaceResult = await SyncWorkspace(configuration.OrganizationUrl, configuration.PersonalAccessToken, workspace.ExternalId, syncId, cancellationToken);
                                    if (syncWorkspaceResult.IsFailure)
                                    {
                                        _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId}. Error: {Error}", workspaceId, syncWorkspaceResult.Error);
                                        continue;
                                    }

                                    _workspaceSynced(_logger, workspaceId, null);

                                    activeWorkspacesSyncedCount++;

                                    var workspaceTeams = workspaceTeamsLookup.TryGetValue(workspace.ExternalId, out var wt) ? wt : Array.Empty<AzureDevOpsBoardsWorkspaceTeamDto>();

                                    var syncIterationsResult = await SyncIterations(configuration.OrganizationUrl, configuration.PersonalAccessToken, workspace.Name, workspaceTeams, connection.SystemId!, syncId, cancellationToken);
                                    if (syncIterationsResult.IsFailure)
                                    {
                                        _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId} iterations. Error: {Error}", workspaceId, syncIterationsResult.Error);
                                        continue;
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

                                        var syncWorkItemsResult = await SyncWorkItems(configuration.OrganizationUrl, configuration.PersonalAccessToken, lastChangedDate, workspaceId, workspace.Name, workspaceTeams, connectionDetails.SystemId!, workTypeNames, syncId, cancellationToken);
                                        if (syncWorkItemsResult.IsFailure)
                                        {
                                            _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId} work items. Error: {Error}", workspaceId, syncWorkItemsResult.Error);
                                            continue;
                                        }

                                        if (syncType == SyncType.Differential)
                                        {
                                            var syncWorkItemParentChangesResult = await SyncWorkItemParentChanges(configuration.OrganizationUrl, configuration.PersonalAccessToken, lastChangedDate, workspaceId, workspace.Name, workTypeNames, syncId, cancellationToken);
                                            if (syncWorkItemParentChangesResult.IsFailure)
                                            {
                                                _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId} work item parent changes. Error: {Error}", workspaceId, syncWorkItemParentChangesResult.Error);
                                                continue;
                                            }
                                        }

                                        var syncWorkItemDependencyChangesResult = await SyncWorkItemDependencyChanges(configuration.OrganizationUrl, configuration.PersonalAccessToken, lastChangedDate, workspaceId, workspace.Name, workTypeNames, syncId, cancellationToken);
                                        if (syncWorkItemDependencyChangesResult.IsFailure)
                                        {
                                            _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId} work item dependency changes. Error: {Error}", workspaceId, syncWorkItemDependencyChangesResult.Error);
                                            continue;
                                        }

                                        var syncDeletedWorkItemsResult = await SyncDeletedWorkItems(configuration.OrganizationUrl, configuration.PersonalAccessToken, workspaceId, workspace.Name, lastChangedDate, syncId, cancellationToken);
                                        if (syncDeletedWorkItemsResult.IsFailure)
                                        {
                                            _logger.LogError("An error occurred while syncing Azure DevOps Boards workspace {WorkspaceId} deleted work items. Error: {Error}", workspaceId, syncDeletedWorkItemsResult.Error);
                                            continue;
                                        }

                                        _workspaceWorkItemsSynced(_logger, workspaceId, null);
                                    }
                                    catch (ValidationException ex)
                                    {
                                        _logger.LogError(ex, "A validation exception occurred while syncing Azure DevOps Boards workspace {WorkspaceId} work items.", workspaceId);
                                        continue;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "An exception occurred while syncing Azure DevOps Boards workspace {WorkspaceId} work items.", workspace.IntegrationState!.InternalId);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }

                _summary(_logger, activeWorkProcessesSyncedCount, activeWorkProcessesCount, activeWorkspacesSyncedCount, activeWorkspacesCount, activeConnectionsCount, null);

                return Result.Success();

                static async Task<DateTime> GetWorkspaceMostRecentChangeDate(ISender sender, Guid workspaceId, CancellationToken cancellationToken)
                {
                    var result = await sender.Send(new GetWorkspaceMostRecentChangeDateQuery(workspaceId), cancellationToken);
                    return result.IsSuccess && result.Value != null
                        ? ((Instant)result.Value).ToDateTimeUtc()
                        : _minSyncDate;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while trying to sync Azure DevOps Boards.");
                throw;
            }
        }
    }

    private async Task<Result> SyncWorkProcess(string organizationUrl, string personalAccessToken, Guid workProcessExternalId, Guid workProcessId, Guid syncId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workProcessExternalId, nameof(workProcessExternalId));
        Guard.Against.Default(workProcessId, nameof(workProcessId));

        // get the process, types, states, and workflow
        var processResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetWorkProcess", () => _azureDevOpsService.GetWorkProcess(organizationUrl, personalAccessToken, workProcessExternalId, cancellationToken), syncId);
        if (processResult.IsFailure)
            return processResult.ConvertFailure();

        // create new types
        var workTypes = processResult.Value.WorkTypes.OfType<IExternalWorkType>().ToList();
        if (workTypes.Count != 0)
        {
            var levels = await _sender.Send(new GetWorkTypeLevelsQuery(), cancellationToken);
            if (levels is null)
                return Result.Failure<Guid>("Unable to get work type levels.");

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
                return Result.Failure<Guid>("Unable to get work type levels.");

            var syncWorkTypesResult = await _sender.Send(new SyncExternalWorkTypesCommand(workTypes, defaultLevelId), cancellationToken);
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
                    return createWorkflowResult.ConvertFailure<Guid>();

                workflowMappings.Add(CreateWorkProcessSchemeDto.Create(workType.Name, workType.IsActive, createWorkflowResult.Value));
            }
            else
            {
                var syncWorkflowResult = await _sender.Send(new UpdateExternalWorkflowCommand(scheme.Workflow.Id, scheme.Workflow.Name, scheme.Workflow.Description, workType), cancellationToken);
                if (syncWorkflowResult.IsFailure)
                    return syncWorkflowResult.ConvertFailure<Guid>();

                workflowMappings.Add(CreateWorkProcessSchemeDto.Create(workType.Name, workType.IsActive, scheme.Workflow.Id));
            }
        }

        // update the work process
        var updateWorkProcessResult = await _sender.Send(new UpdateExternalWorkProcessCommand(processResult.Value, processResult.Value.WorkTypes, workflowMappings), cancellationToken);

        return updateWorkProcessResult.IsSuccess
            ? Result.Success()
            : updateWorkProcessResult;
    }

    private async Task<Result> SyncWorkspace(string organizationUrl, string personalAccessToken, Guid workspaceExternalId, Guid syncId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceExternalId, nameof(workspaceExternalId));

        var workspaceResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetWorkspace", () => _azureDevOpsService.GetWorkspace(organizationUrl, personalAccessToken, workspaceExternalId, cancellationToken), syncId);
        if (workspaceResult.IsFailure)
            return workspaceResult.ConvertFailure();

        var updateResult = await _sender.Send(new UpdateExternalWorkspaceCommand(workspaceResult.Value), cancellationToken);

        return updateResult.IsSuccess
            ? Result.Success()
            : updateResult;
    }

    private async Task<Result> SyncIterations(string organizationUrl, string personalAccessToken, string azdoWorkspaceName, AzureDevOpsBoardsWorkspaceTeamDto[] workspaceTeams, string systemId, Guid syncId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.NullOrWhiteSpace(azdoWorkspaceName, nameof(azdoWorkspaceName));

        BuildTeamSettingsAndMappings(workspaceTeams, out var teamSettings, out var teamMappings);

        var iterationsResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetIterations", () => _azureDevOpsService.GetIterations(organizationUrl, personalAccessToken, azdoWorkspaceName, teamSettings, cancellationToken), syncId);
        if (iterationsResult.IsFailure)
            return iterationsResult.ConvertFailure();

        var syncResult = await _sender.Send(new SyncAzureDevOpsIterationsCommand(systemId, iterationsResult.Value, teamMappings), cancellationToken);

        return syncResult;
    }

    private async Task<Result> SyncWorkItems(string organizationUrl, string personalAccessToken, DateTime lastChangedDate, Guid workspaceId, string azdoWorkspaceName, AzureDevOpsBoardsWorkspaceTeamDto[] workspaceTeams, string systemId, string[] workTypeNames, Guid syncId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        BuildTeamSettingsAndMappings(workspaceTeams, out var teamSettings, out var teamMappings);

        var workItemsResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetWorkItems", () => _azureDevOpsService.GetWorkItems(organizationUrl, personalAccessToken, azdoWorkspaceName, lastChangedDate, workTypeNames, teamSettings, cancellationToken), syncId);
        if (workItemsResult.IsFailure)
            return workItemsResult.ConvertFailure();

        _workItemsRetrieved(_logger, workItemsResult.Value.Count, azdoWorkspaceName, null);

        var iterationMappings = await _sender.Send(new GetIterationMappingsQuery(Connector.AzureDevOps, systemId), cancellationToken);

        return workItemsResult.Value.Count == 0
            ? Result.Success()
            : await _sender.Send(new SyncExternalWorkItemsCommand(workspaceId, workItemsResult.Value, teamMappings, iterationMappings), cancellationToken);
    }

    private async Task<Result> SyncWorkItemParentChanges(string organizationUrl, string personalAccessToken, DateTime lastChangedDate, Guid workspaceId, string azdoWorkspaceName, string[] workTypeNames, Guid syncId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var parentLinkChangesResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetParentLinkChanges", () => _azureDevOpsService.GetParentLinkChanges(organizationUrl, personalAccessToken, azdoWorkspaceName, lastChangedDate, workTypeNames, cancellationToken), syncId);
        if (parentLinkChangesResult.IsFailure)
            return parentLinkChangesResult.ConvertFailure();

        _parentChangesRetrieved(_logger, parentLinkChangesResult.Value.Count, azdoWorkspaceName, null);

        return parentLinkChangesResult.Value.Count == 0
            ? Result.Success()
            : await _sender.Send(new SyncExternalWorkItemParentChangesCommand(workspaceId, parentLinkChangesResult.Value), cancellationToken);
    }

    private async Task<Result> SyncWorkItemDependencyChanges(string organizationUrl, string personalAccessToken, DateTime lastChangedDate, Guid workspaceId, string azdoWorkspaceName, string[] workTypeNames, Guid syncId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var dependencyLinkChangesResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetDependencyLinkChanges", () => _azureDevOpsService.GetDependencyLinkChanges(organizationUrl, personalAccessToken, azdoWorkspaceName, lastChangedDate, workTypeNames, cancellationToken), syncId);
        if (dependencyLinkChangesResult.IsFailure)
            return dependencyLinkChangesResult.ConvertFailure();

        _dependencyChangesRetrieved(_logger, dependencyLinkChangesResult.Value.Count, azdoWorkspaceName, null);

        return dependencyLinkChangesResult.Value.Count == 0
            ? Result.Success()
            : await _sender.Send(new SyncExternalWorkItemDependencyChangesCommand(workspaceId, dependencyLinkChangesResult.Value), cancellationToken);
    }

    private async Task<Result> SyncDeletedWorkItems(string organizationUrl, string personalAccessToken, Guid workspaceId, string azdoWorkspaceName, DateTime lastChangedDate, Guid syncId, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(organizationUrl, nameof(organizationUrl));
        Guard.Against.NullOrWhiteSpace(personalAccessToken, nameof(personalAccessToken));
        Guard.Against.Default(workspaceId, nameof(workspaceId));

        var getDeletedWorkItemIdsResult = await ExternalCallMeasure.MeasureAsync(_logger, "Azdo_Sync_GetDeletedWorkItemIds", () => _azureDevOpsService.GetDeletedWorkItemIds(organizationUrl, personalAccessToken, azdoWorkspaceName, lastChangedDate, cancellationToken), syncId);
        if (getDeletedWorkItemIdsResult.IsFailure)
            return getDeletedWorkItemIdsResult.ConvertFailure();

        _deletedWorkItemsRetrieved(_logger, getDeletedWorkItemIdsResult.Value.Length, azdoWorkspaceName, null);

        return getDeletedWorkItemIdsResult.Value.Length == 0
            ? Result.Success()
            : await _sender.Send(new DeleteExternalWorkItemsCommand(workspaceId, getDeletedWorkItemIdsResult.Value), cancellationToken);
    }

    private static void BuildTeamSettingsAndMappings(AzureDevOpsBoardsWorkspaceTeamDto[] workspaceTeams, out Dictionary<Guid, Guid?> teamSettings, out Dictionary<Guid, Guid?> teamMappings)
    {
        if (workspaceTeams == null || workspaceTeams.Length == 0)
        {
            teamSettings = [];
            teamMappings = [];
            return;
        }

        teamSettings = new Dictionary<Guid, Guid?>(workspaceTeams.Length);
        teamMappings = new Dictionary<Guid, Guid?>(workspaceTeams.Length);

        foreach (var team in workspaceTeams)
        {
            if (team.InternalTeamId is null)
                continue;

            teamSettings[team.TeamId] = team.BoardId;
            teamMappings[team.TeamId] = team.InternalTeamId;
        }
    }
}
