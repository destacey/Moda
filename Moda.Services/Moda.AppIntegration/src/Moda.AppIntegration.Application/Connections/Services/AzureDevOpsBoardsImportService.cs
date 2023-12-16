using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moda.AppIntegration.Application.Connections.Commands;
using Moda.AppIntegration.Application.Connections.Queries;
using Moda.AppIntegration.Application.Interfaces;
using Moda.Work.Application.Workspaces.Queries;
using Moda.Work.Domain.Models;

namespace Moda.AppIntegration.Application.Connections.Services;
public sealed class AzureDevOpsBoardsImportService : IAzureDevOpsBoardsImportService
{
    private readonly ILogger<AzureDevOpsBoardsImportService> _logger;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly ISender _sender;
    private readonly IDateTimeService _dateTimeService;

    public AzureDevOpsBoardsImportService(ILogger<AzureDevOpsBoardsImportService> logger, IAzureDevOpsService azureDevOpsService, ISender sender, IDateTimeService dateTimeService)
    {
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
        _sender = sender;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result> ImportWorkspaces(Guid connectionId, CancellationToken cancellationToken)
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

            List<AzureDevOpsBoardsWorkProcess> processes = new();
            foreach (var externalProcess in workProcessesResult.Value)
            {
                var process = AzureDevOpsBoardsWorkProcess.Create(externalProcess.Id, externalProcess.Name, externalProcess.Description);
                processes.Add(process);
            }

            // Load workspaces
            var workspacesResult = await _azureDevOpsService.GetWorkspaces(connection.Configuration.OrganizationUrl, connection.Configuration.PersonalAccessToken);
            if (workspacesResult.IsFailure)
                return workspacesResult;

            List<AzureDevOpsBoardsWorkspace> workspaces = new();
            foreach (var externalWorkspace in workspacesResult.Value)
            {
                var workProcessId = workProcessesResult.Value.FirstOrDefault(p => p.WorkspaceIds.Contains(externalWorkspace.Id))?.Id;
                var workspace = AzureDevOpsBoardsWorkspace.Create(externalWorkspace.Id, externalWorkspace.Name, externalWorkspace.Description, workProcessId);
                workspaces.Add(workspace);
            }

            var bulkUpsertResult = await _sender.Send(new BulkUpsertAzureDevOpsBoardsWorkspacesCommand(connectionId, processes, workspaces), cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to import projects from Azure DevOps for connection {ConnectionId}.", connectionId);
            return Result.Failure($"An error occurred while trying to import projects from Azure DevOps for connection {connectionId}.");
        }

    }

    public async Task<Result> InitWorkspaceIntegration(Guid connectionId, Guid workspaceExternalId, string workspaceKey, CancellationToken cancellationToken)
    {
        try
        {
            var connectionCheckResult = await GetValidConnectionAndWorkspace(connectionId, workspaceExternalId, cancellationToken);
            if (connectionCheckResult.IsFailure)
                return connectionCheckResult;

            // TODO: should the lookup be on the external id and connector? azdo|externalId
            // TODO: crossing service boundaries :(
            // verify the externalId isn't already integrated
            var exists = await _sender.Send(new ExternalWorkspaceExistsQuery(workspaceExternalId), cancellationToken);
            if (exists)
            {
                _logger.LogError("Unable to initialize a workspace {WorkspaceExternalId} from Azure DevOps for connection {ConnectionId} because it is already integrated.", workspaceExternalId, connectionId);
                return Result.Failure($"Unable to initialize a workspace {workspaceExternalId} from Azure DevOps for connection {connectionId} because it is already integrated.");
            }

            var isDuplicateKey = await _sender.Send(new WorkspaceKeyExistsQuery(new WorkspaceKey(workspaceKey)), cancellationToken);
            if (isDuplicateKey)
            {
                // TODO: should this be a validation exception
                _logger.LogWarning("Unable to initialize a workspace {WorkspaceExternalId} from Azure DevOps for connection {ConnectionId} because the workspace key {WorkspaceKey} is already in use.", workspaceExternalId, connectionId, workspaceKey);
                return Result.Failure($"Unable to initialize a workspace {workspaceExternalId} from Azure DevOps for connection {connectionId} because the workspace key {workspaceKey} is already in use.");
            }

            // re-import the connection to make sure everything is up-to-date
            var importWorkspacesResult = await ImportWorkspaces(connectionId, cancellationToken);
            if (importWorkspacesResult.IsFailure)
                return importWorkspacesResult;

            // getting the connection again to make sure we have the latest data after the import
            var connectionResult = await GetValidConnectionAndWorkspace(connectionId, workspaceExternalId, cancellationToken);
            if (connectionResult.IsFailure)
                return connectionResult;

            // get the process, types, and states



            return Result.Failure("Integration initialization still in progress.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to initialize a project {WorkspaceExternalId} from Azure DevOps for connection {ConnectionId}.", workspaceExternalId, connectionId);
            return Result.Failure($"An error occurred while trying to initialize a project {workspaceExternalId} from Azure DevOps for connection {connectionId}.");
        }

    }

    private async Task<Result<AzureDevOpsBoardsConnectionDetailsDto>> GetValidConnectionAndWorkspace(Guid connectionId, Guid workspaceExternalId, CancellationToken cancellationToken)
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
        else if (connection.Configuration.Workspaces.All(w => w.ExternalId != workspaceExternalId))
        {
            _logger.LogError("The workspace {WorkspaceExternalId} is not linked to connection {ConnectionId}.", workspaceExternalId, connectionId);
            return Result.Failure<AzureDevOpsBoardsConnectionDetailsDto>($"The workspace {workspaceExternalId} is not linked to connection {connectionId}.");
        }

        return Result.Success(connection);
    }
}
