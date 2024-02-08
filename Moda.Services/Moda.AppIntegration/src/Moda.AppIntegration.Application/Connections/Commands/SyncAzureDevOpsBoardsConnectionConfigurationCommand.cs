using Microsoft.EntityFrameworkCore;
using Moda.Common.Domain.Models;

namespace Moda.AppIntegration.Application.Connections.Commands;

public sealed record SyncAzureDevOpsBoardsConnectionConfigurationCommand(Guid ConnectionId, IEnumerable<AzureDevOpsBoardsWorkProcess> WorkProcesses, IEnumerable<AzureDevOpsBoardsWorkspace> Workspaces, IEnumerable<IntegrationRegistration<Guid, Guid>> WorkProcessIntegrationRegistrations) : ICommand;

internal sealed class SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler : ICommandHandler<SyncAzureDevOpsBoardsConnectionConfigurationCommand>
{
    private const string AppRequestName = nameof(SyncAzureDevOpsBoardsConnectionConfigurationCommand);

    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler> _logger;

    public SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeProvider, ILogger<SyncAzureDevOpsBoardsConnectionConfigurationCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeProvider = dateTimeProvider;
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

        var timestamp = _dateTimeProvider.Now;

        var importWorkProcessesResult = connection.SyncProcesses(request.WorkProcesses, timestamp);
        if (importWorkProcessesResult.IsFailure)
        {
            _logger.LogError("Errors occurred while processing {AppRequestName}. {Error}", AppRequestName, importWorkProcessesResult.Error);

            return Result.Failure($"Errors occurred while processing {AppRequestName}.");
        }

        foreach (var workProcess in connection.Configuration.WorkProcesses)
        {
            var workProcessIntegrationRegistration = request.WorkProcessIntegrationRegistrations.FirstOrDefault(w => w.ExternalId == workProcess.ExternalId);
            if (workProcessIntegrationRegistration is null) continue;

            var result = connection.UpdateWorkProcessIntegrationState(workProcessIntegrationRegistration, timestamp);
            if (result.IsFailure)
            {
                _logger.LogError("Errors occurred while processing {AppRequestName}. {Error}", AppRequestName, result.Error);

                return Result.Failure($"Errors occurred while processing {AppRequestName}.");
            }

        }

        var importWorkspacesResult = connection.SyncWorkspaces(request.Workspaces, timestamp);
        if (importWorkspacesResult.IsFailure)
        {
            _logger.LogError("Errors occurred while processing {AppRequestName}. {Error}", AppRequestName, importWorkspacesResult.Error);

            return Result.Failure($"Errors occurred while processing {AppRequestName}.");
        }

        await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}