using MediatR;
using Moda.AppIntegration.Application.Connections.Commands;
using Moda.AppIntegration.Application.Connections.Queries;
using Moda.AppIntegration.Application.Interfaces;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Requests.WorkManagement;
using Moda.Common.Domain.Models;
using Moda.Common.Models;
using Moda.Work.Application.Workflows.Dtos;
using Moda.Work.Application.WorkProcesses.Queries;
using Moda.Work.Application.Workspaces.Queries;
using Moda.Work.Application.WorkStatuses.Commands;
using Moda.Work.Application.WorkTypes.Commands;

namespace Moda.AppIntegration.Application.Connections.Managers;
public sealed class AzureDevOpsBoardsInitManager(ILogger<AzureDevOpsBoardsInitManager> logger, IAzureDevOpsService azureDevOpsService, ISender sender) : IAzureDevOpsBoardsInitManager
{
    private readonly ILogger<AzureDevOpsBoardsInitManager> _logger = logger;
    private readonly IAzureDevOpsService _azureDevOpsService = azureDevOpsService;
    private readonly ISender _sender = sender;

    public async Task<Result> SyncOrganizationConfiguration(Guid connectionId, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _sender.Send(new GetAzureDevOpsBoardsConnectionQuery(connectionId), cancellationToken);
            if (connection is null)
            {
                _logger.LogError("Unable to find Azure DevOps connection with id {ConnectionId}.", connectionId);
                return Result.Failure($"Unable to find Azure DevOps connection with id {connectionId}.");
            }

            if (connection.IsValidConfiguration is false)
            {
                _logger.LogError("The configuration for Azure DevOps connection {ConnectionId} is not valid.", connectionId);
                return Result.Failure($"The configuration for connection {connectionId} is not valid.");
            }

            // Load Processes
            var workProcessesResult = await _azureDevOpsService.GetWorkProcesses(connection.Configuration.OrganizationUrl, connection.Configuration.PersonalAccessToken, cancellationToken);
            if (workProcessesResult.IsFailure)
                return workProcessesResult;

            List<AzureDevOpsBoardsWorkProcess> processes = [];
            foreach (var externalProcess in workProcessesResult.Value)
            {
                var process = AzureDevOpsBoardsWorkProcess.Create(externalProcess.Id, externalProcess.Name, externalProcess.Description);
                processes.Add(process);
            }

            // Load workspaces
            var workspacesResult = await _azureDevOpsService.GetWorkspaces(connection.Configuration.OrganizationUrl, connection.Configuration.PersonalAccessToken, cancellationToken);
            if (workspacesResult.IsFailure)
                return workspacesResult;

            List<AzureDevOpsBoardsWorkspace> workspaces = [];
            foreach (var externalWorkspace in workspacesResult.Value)
            {
                var workProcessId = workProcessesResult.Value.FirstOrDefault(p => p.WorkspaceIds.Contains(externalWorkspace.Id))?.Id;
                var workspace = AzureDevOpsBoardsWorkspace.Create(externalWorkspace.Id, externalWorkspace.Name, externalWorkspace.Description, workProcessId);
                workspaces.Add(workspace);
            }

            var existingWorkProcessIntegrationStates = await _sender.Send(new GetIntegrationRegistrationsForWorkProcessesQuery(), cancellationToken);


            var bulkUpsertResult = await _sender.Send(new SyncAzureDevOpsBoardsConnectionConfigurationCommand(connectionId, processes, workspaces, existingWorkProcessIntegrationStates), cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to import projects from Azure DevOps for connection {ConnectionId}.", connectionId);
            return Result.Failure($"An error occurred while trying to import projects from Azure DevOps for connection {connectionId}.");
        }

    }

    public async Task<Result<Guid>> InitWorkProcessIntegration(Guid connectionId, Guid workProcessExternalId, CancellationToken cancellationToken)
    {
        try
        {
            var connectionCheckResult = await GetValidConnectionDetailsForWorkProcess(connectionId, workProcessExternalId, cancellationToken);
            if (connectionCheckResult.IsFailure)
                return connectionCheckResult.ConvertFailure<Guid>();

            // TODO: should the lookup be on the external id and connector? azdo|externalId
            // TODO: crossing service boundaries :(
            // verify the externalId isn't already integrated
            var exists = await _sender.Send(new ExternalWorkProcessExistsQuery(workProcessExternalId), cancellationToken);
            if (exists)
            {
                _logger.LogError("Unable to initialize a work process {WorkProcessExternalId} from Azure DevOps for connection {ConnectionId} because it is already integrated.", workProcessExternalId, connectionId);
                return Result.Failure<Guid>($"Unable to initialize a work process {workProcessExternalId} from Azure DevOps for connection {connectionId} because it is already integrated.");
            }

            // re-import the connection to make sure everything is up-to-date
            var importWorkspacesResult = await SyncOrganizationConfiguration(connectionId, cancellationToken);
            if (importWorkspacesResult.IsFailure)
                return importWorkspacesResult.ConvertFailure<Guid>();

            // getting the connection again to make sure we have the latest data after the import
            var connectionResult = await GetValidConnectionDetailsForWorkProcess(connectionId, workProcessExternalId, cancellationToken);
            if (connectionResult.IsFailure)
                return connectionResult.ConvertFailure<Guid>();

            // get the process, types, states, and workflow
            var processResult = await _azureDevOpsService.GetWorkProcess(connectionResult.Value.Configuration.OrganizationUrl, connectionResult.Value.Configuration.PersonalAccessToken, workProcessExternalId, cancellationToken);
            if (processResult.IsFailure)
                return processResult.ConvertFailure<Guid>();

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

            // create workflow for each work type and map the workflow id to the type
            var workProcessSchemes = new List<CreateWorkProcessSchemeDto>(processResult.Value.WorkTypes.Count);
            foreach (var workType in processResult.Value.WorkTypes)
            {
                var createWorkflowResult = await _sender.Send(new CreateExternalWorkflowCommand(
                    $"{processResult.Value.Name} - {workType.Name}",
                    "Auto-generated workflow for Azure DevOps work process.",
                    workType), cancellationToken);
                if (createWorkflowResult.IsFailure)
                    return createWorkflowResult.ConvertFailure<Guid>();

                workProcessSchemes.Add(CreateWorkProcessSchemeDto.Create(workType.Name, workType.IsActive, createWorkflowResult.Value));
            }

            // create process
            var createProcessResult = await _sender.Send(new CreateExternalWorkProcessCommand(processResult.Value, workProcessSchemes), cancellationToken);
            if (createProcessResult.IsFailure)
                return createProcessResult.ConvertFailure<Guid>();

            // update the integration state
            var updateIntegrationStateResult = await _sender.Send(new UpdateAzureDevOpsBoardsWorkProcessIntegrationStateCommand(connectionId, new IntegrationRegistration<Guid, Guid>(workProcessExternalId, createProcessResult.Value)), cancellationToken);

            return updateIntegrationStateResult.IsSuccess
                ? Result.Success(createProcessResult.Value.InternalId)
                : updateIntegrationStateResult.ConvertFailure<Guid>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to initialize a work process {WorkProcessExternalId} from Azure DevOps for connection {ConnectionId}.", workProcessExternalId, connectionId);
            return Result.Failure<Guid>($"An error occurred while trying to initialize a work process {workProcessExternalId} from Azure DevOps for connection {connectionId}.");
        }
    }

    public async Task<Result<Guid>> InitWorkspaceIntegration(Guid connectionId, Guid workspaceExternalId, string workspaceKey, string workspaceName, string? externalViewWorkItemUrlTemplate, CancellationToken cancellationToken)
    {
        try
        {
            var connectionCheckResult = await GetValidConnectionDetailsForWorkspace(connectionId, workspaceExternalId, cancellationToken);
            if (connectionCheckResult.IsFailure)
                return connectionCheckResult.ConvertFailure<Guid>();

            // TODO: should the lookup be on the external id and connector? azdo|externalId
            // TODO: crossing service boundaries :(
            // verify the externalId isn't already integrated
            var exists = await _sender.Send(new ExternalWorkspaceExistsQuery(workspaceExternalId), cancellationToken);
            if (exists)
            {
                _logger.LogError("Unable to initialize a workspace {WorkspaceExternalId} from Azure DevOps for connection {ConnectionId} because it is already integrated.", workspaceExternalId, connectionId);
                return Result.Failure<Guid>($"Unable to initialize a workspace {workspaceExternalId} from Azure DevOps for connection {connectionId} because it is already integrated.");
            }

            var isDuplicateKey = await _sender.Send(new WorkspaceKeyExistsQuery(workspaceKey), cancellationToken);
            if (isDuplicateKey)
            {
                // TODO: should this be a validation exception
                _logger.LogWarning("Unable to initialize a workspace {WorkspaceExternalId} from Azure DevOps for connection {ConnectionId} because the workspace key {WorkspaceKey} is already in use.", workspaceExternalId, connectionId, workspaceKey);
                return Result.Failure<Guid>($"Unable to initialize a workspace {workspaceExternalId} from Azure DevOps for connection {connectionId} because the workspace key {workspaceKey} is already in use.");
            }

            // re-import the connection to make sure everything is up-to-date
            var importWorkspacesResult = await SyncOrganizationConfiguration(connectionId, cancellationToken);
            if (importWorkspacesResult.IsFailure)
                return importWorkspacesResult.ConvertFailure<Guid>();

            // getting the connection again to make sure we have the latest data after the import
            var connectionResult = await GetValidConnectionDetailsForWorkspace(connectionId, workspaceExternalId, cancellationToken);
            if (connectionResult.IsFailure)
                return connectionResult.ConvertFailure<Guid>();

            // get the workspace
            var workspaceResult = await _azureDevOpsService.GetWorkspace(connectionResult.Value.Configuration.OrganizationUrl, connectionResult.Value.Configuration.PersonalAccessToken, workspaceExternalId, cancellationToken);
            if (workspaceResult.IsFailure)
                return workspaceResult.ConvertFailure<Guid>();

            // create the workspace
            var createWorkspaceResult = await _sender.Send(new CreateExternalWorkspaceCommand(workspaceResult.Value, new WorkspaceKey(workspaceKey), workspaceName, externalViewWorkItemUrlTemplate), cancellationToken);
            if (createWorkspaceResult.IsFailure)
                return createWorkspaceResult.ConvertFailure<Guid>();

            // update the integration state
            var updateIntegrationStateResult = await _sender.Send(new UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommand(connectionId, new IntegrationRegistration<Guid, Guid>(workspaceExternalId, createWorkspaceResult.Value)), cancellationToken);

            return updateIntegrationStateResult.IsSuccess
                ? Result.Success(createWorkspaceResult.Value.InternalId)
                : updateIntegrationStateResult.ConvertFailure<Guid>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to initialize a workspace {WorkspaceExternalId} from Azure DevOps for connection {ConnectionId}.", workspaceExternalId, connectionId);
            return Result.Failure<Guid>($"An error occurred while trying to initialize a workspace {workspaceExternalId} from Azure DevOps for connection {connectionId}.");
        }
    }

    private async Task<Result<AzureDevOpsBoardsConnectionDetailsDto>> GetValidConnectionDetailsForWorkProcess(Guid connectionId, Guid workProcessExternalId, CancellationToken cancellationToken)
    {
        var result = await GetValidConnectionDetails(connectionId, cancellationToken);
        if (result.IsFailure)
            return result;

        if (result.Value.Configuration.WorkProcesses.All(w => w.ExternalId != workProcessExternalId))
        {
            _logger.LogError("The work proces {WorkProcessExternalId} is not linked to connection {ConnectionId}.", workProcessExternalId, connectionId);
            return Result.Failure<AzureDevOpsBoardsConnectionDetailsDto>($"The work proces {workProcessExternalId} is not linked to connection {connectionId}.");
        }

        return Result.Success(result.Value);
    }

    private async Task<Result<AzureDevOpsBoardsConnectionDetailsDto>> GetValidConnectionDetailsForWorkspace(Guid connectionId, Guid workspaceExternalId, CancellationToken cancellationToken)
    {
        var result = await GetValidConnectionDetails(connectionId, cancellationToken);
        if (result.IsFailure)
            return result;

        if (result.Value.Configuration.Workspaces.All(w => w.ExternalId != workspaceExternalId))
        {
            _logger.LogError("The workspace {WorkspaceExternalId} is not linked to connection {ConnectionId}.", workspaceExternalId, connectionId);
            return Result.Failure<AzureDevOpsBoardsConnectionDetailsDto>($"The workspace {workspaceExternalId} is not linked to connection {connectionId}.");
        }

        return Result.Success(result.Value);
    }

    private async Task<Result<AzureDevOpsBoardsConnectionDetailsDto>> GetValidConnectionDetails(Guid connectionId, CancellationToken cancellationToken)
    {
        var connection = await _sender.Send(new GetAzureDevOpsBoardsConnectionQuery(connectionId), cancellationToken);
        if (connection is null)
        {
            _logger.LogError("Unable to find Azure DevOps connection with id {ConnectionId}.", connectionId);
            return Result.Failure<AzureDevOpsBoardsConnectionDetailsDto>($"Unable to find Azure DevOps connection with id {connectionId}.");
        }
        else if (connection.IsValidConfiguration is false || connection.IsActive is false)
        {
            _logger.LogError("The configuration for Azure DevOps connection {ConnectionId} is not valid.", connectionId);
            return Result.Failure<AzureDevOpsBoardsConnectionDetailsDto>($"The configuration for connection {connectionId} is not valid.");
        }

        return Result.Success(connection);
    }
}
