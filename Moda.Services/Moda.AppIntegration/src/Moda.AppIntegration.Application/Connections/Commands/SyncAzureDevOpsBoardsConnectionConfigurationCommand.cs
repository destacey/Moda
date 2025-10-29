using MediatR;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Requests.WorkManagement;
using Moda.Common.Domain.Models;
using NodaTime;

namespace Moda.AppIntegration.Application.Connections.Commands;

public sealed record SyncAzureDevOpsBoardsConnectionConfigurationCommand(
    Guid ConnectionId,
    IEnumerable<AzureDevOpsBoardsWorkProcess> WorkProcesses,
    IEnumerable<AzureDevOpsBoardsWorkspace> Workspaces,
    IEnumerable<IntegrationRegistration<Guid, Guid>> WorkProcessIntegrationRegistrations,
    List<IExternalTeam> Teams)
    : ICommand;

internal sealed class SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeProvider, ILogger<SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler> logger, IAzureDevOpsService azureDevOpsService, ISender sender) : ICommandHandler<SyncAzureDevOpsBoardsConnectionConfigurationCommand>
{
    private const string AppRequestName = nameof(SyncAzureDevOpsBoardsConnectionConfigurationCommand);

    private readonly IAppIntegrationDbContext _appIntegrationDbContext = appIntegrationDbContext;
    private readonly Instant _timestamp = dateTimeProvider.Now;
    private readonly ILogger<SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler> _logger = logger;
    private readonly IAzureDevOpsService _azureDevOpsService = azureDevOpsService;
    private readonly ISender _sender = sender;

    public async Task<Result> Handle(SyncAzureDevOpsBoardsConnectionConfigurationCommand request, CancellationToken cancellationToken)
    {
        var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections.FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            _logger.LogError("Unable to find Azure DevOps Boards connection with id {ConnectionId}.", request.ConnectionId);
            return Result.Failure($"Unable to find Azure DevOps Boards connection with id {request.ConnectionId}.");
        }


        // this is a temporary process to set the SystemId if it is missing
        if (string.IsNullOrWhiteSpace(connection.SystemId))
        {
            var setSystemIdResult = await SetConnectionSystemId(connection, cancellationToken);
            if (setSystemIdResult.IsFailure)
                return setSystemIdResult;

            await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Set SystemId for Azure DevOps Boards connection {ConnectionId} to {SystemId}.", connection.Id, connection.SystemId);
        }

        // get workspace internal ids after setting system id
        var workspaceIds = connection.Configuration.Workspaces.Where(w => w.IntegrationState?.InternalId != null).Select(w => w.IntegrationState!.InternalId).ToList();

        var setSystemIdOnWorkspacesResult = await _sender.Send(new SetSystemIdOnExternalWorkspacesCommand(workspaceIds, connection.Connector, connection.SystemId!), cancellationToken);
        if (setSystemIdOnWorkspacesResult.IsFailure)
        {
            _logger.LogError("Failed to set SystemId on external workspaces for connection {ConnectionId}. {Error}", connection.Id, setSystemIdOnWorkspacesResult.Error);
            return Result.Failure($"Failed to set SystemId on external workspaces for connection {connection.Id}. {setSystemIdOnWorkspacesResult.Error}");
        }


        var importWorkProcessesResult = connection.SyncProcesses(request.WorkProcesses, _timestamp);
        if (importWorkProcessesResult.IsFailure)
        {
            return LogAndReturnFailure(importWorkProcessesResult);
        }

        foreach (var workProcess in connection.Configuration.WorkProcesses)
        {
            var workProcessIntegrationRegistration = request.WorkProcessIntegrationRegistrations.FirstOrDefault(w => w.ExternalId == workProcess.ExternalId);
            if (workProcessIntegrationRegistration is null) continue;

            var result = connection.UpdateWorkProcessIntegrationState(workProcessIntegrationRegistration, _timestamp);
            if (result.IsFailure)
            {
                return LogAndReturnFailure(result);
            }
        }

        var importWorkspacesResult = connection.SyncWorkspaces(request.Workspaces, _timestamp);
        if (importWorkspacesResult.IsFailure)
        {
            return LogAndReturnFailure(importWorkspacesResult);
        }

        var syncTeamsResult = connection.SyncTeams(request.Teams, _timestamp);
        if (syncTeamsResult.IsFailure)
        {
            return LogAndReturnFailure(syncTeamsResult);
        }

        await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();

        Result LogAndReturnFailure(Result result)
        {
            _logger.LogError("Errors occurred while processing {AppRequestName}. {Error}", AppRequestName, result.Error);

            return Result.Failure($"Errors occurred while processing {AppRequestName}.");
        }
    }

    private async Task<Result> SetConnectionSystemId(AzureDevOpsBoardsConnection connection, CancellationToken cancellationToken)
    {
        var config = new AzureDevOpsBoardsConnectionConfiguration(connection.Configuration.Organization, connection.Configuration.PersonalAccessToken);

        var systemIdResult = await _azureDevOpsService.GetSystemId(config.OrganizationUrl, config.PersonalAccessToken, cancellationToken);
        if (systemIdResult.IsFailure)
        {
            _logger.LogError("Unable to get system id for Azure DevOps Boards connection {ConnectionId}. {Error}", connection.Id, systemIdResult.Error);
            return Result.Failure($"Unable to get system id for Azure DevOps Boards connection for organization {connection.Configuration.Organization}.");
        }

        var setSystemIdResult = connection.SetSystemId(systemIdResult.Value);
        if (setSystemIdResult.IsFailure)
        {
            _logger.LogError("Error setting system id for connection {ConnectionId} to {SystemId}. {Error}", connection.Id, systemIdResult.Value, setSystemIdResult.Error);
            return Result.Failure($"Error setting system id for connection {connection.Id} to {systemIdResult.Value}. {setSystemIdResult.Error}");
        }

        return Result.Success();
    }
}