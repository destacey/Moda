﻿using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connections.Commands;

public sealed record BulkUpsertAzureDevOpsBoardsWorkspacesCommand(Guid ConnectionId, IEnumerable<AzureDevOpsBoardsWorkProcess> WorkProcesses, IEnumerable<AzureDevOpsBoardsWorkspace> Workspaces) : ICommand;

internal sealed class BulkUpsertAzureDevOpsBoardsWorkspacesCommandHandler : ICommandHandler<BulkUpsertAzureDevOpsBoardsWorkspacesCommand>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<BulkUpsertAzureDevOpsBoardsWorkspacesCommandHandler> _logger;

    public BulkUpsertAzureDevOpsBoardsWorkspacesCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeProvider, ILogger<BulkUpsertAzureDevOpsBoardsWorkspacesCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(BulkUpsertAzureDevOpsBoardsWorkspacesCommand request, CancellationToken cancellationToken)
    {
        var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections.FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            _logger.LogError("Unable to find Azure DevOps Boards connection with id {ConnectionId}.", request.ConnectionId);
            return Result.Failure($"Unable to find Azure DevOps Boards connection with id {request.ConnectionId}.");
        }

        var importWorkProcessesResult = connection.ImportProcesses(request.WorkProcesses, _dateTimeProvider.Now);
        if (importWorkProcessesResult.IsFailure)
        {
            string requestName = request.GetType().Name;
            _logger.LogError("Errors occurred while processing {RequestName}. {Error}", requestName, importWorkProcessesResult.Error);

            return Result.Failure($"Errors occurred while processing {requestName}.");
        }

        var importWorkspacesResult = connection.ImportWorkspaces(request.Workspaces, _dateTimeProvider.Now);
        if (importWorkspacesResult.IsFailure)
        {
            string requestName = request.GetType().Name;
            _logger.LogError("Errors occurred while processing {RequestName}. {Error}", requestName, importWorkspacesResult.Error);

            return Result.Failure($"Errors occurred while processing {requestName}.");
        }

        await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}