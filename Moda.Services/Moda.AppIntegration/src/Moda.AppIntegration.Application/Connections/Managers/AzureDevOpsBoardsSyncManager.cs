using Ardalis.GuardClauses;
using MediatR;
using Moda.AppIntegration.Application.Connections.Queries;
using Moda.AppIntegration.Application.Interfaces;
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

            foreach (var connection in connections.Where(c => c.IsValidConfiguration && c.IsSyncEnabled))
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

                foreach (var workProcess in connectionDetails.Configuration.WorkProcesses)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        var message = "Cancellation requested. Stopping sync.";
                        _logger.LogInformation(message);
                        return Result.Success();
                    }

                    if (workProcess.IntegrationState is null || !workProcess.IntegrationState.IsActive)
                        continue;

                    var syncResult = await SyncWorkProcess(connectionDetails.Configuration.OrganizationUrl, connectionDetails.Configuration.PersonalAccessToken, workProcess.ExternalId, cancellationToken);
                    if (syncResult.IsFailure)
                    {
                        _logger.LogError("An error occurred while syncing Azure DevOps Boards work process with ID {WorkProcessId}. Error: {Error}", workProcess.Id, syncResult.Error);
                        continue;
                    }
                    else
                    {
                        _logger.LogInformation("Successfully synced Azure DevOps Boards work process with ID {WorkProcessId}.", workProcess.Id);
                    }

                    // TODO: sync workspaces
                }
            }

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
}
