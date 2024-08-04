using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Models;
using NodaTime;

namespace Moda.AppIntegration.Application.Connections.Commands;

public sealed record SyncAzureDevOpsBoardsConnectionConfigurationCommand(Guid ConnectionId, IEnumerable<AzureDevOpsBoardsWorkProcess> WorkProcesses, IEnumerable<AzureDevOpsBoardsWorkspace> Workspaces, IEnumerable<IntegrationRegistration<Guid, Guid>> WorkProcessIntegrationRegistrations, List<IExternalTeam> Teams) : ICommand;

internal sealed class SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler : ICommandHandler<SyncAzureDevOpsBoardsConnectionConfigurationCommand>
{
    private const string AppRequestName = nameof(SyncAzureDevOpsBoardsConnectionConfigurationCommand);

    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly Instant _timestamp;
    private readonly ILogger<SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler> _logger;

    public SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeProvider, ILogger<SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _timestamp = dateTimeProvider.Now;
        _logger = logger;
    }

    public async Task<Result> Handle(SyncAzureDevOpsBoardsConnectionConfigurationCommand request, CancellationToken cancellationToken)
    {
        var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections.FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            _logger.LogError("Unable to find Azure DevOps Boards connection with id {ConnectionId}.", request.ConnectionId);
            return Result.Failure($"Unable to find Azure DevOps Boards connection with id {request.ConnectionId}.");
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
}