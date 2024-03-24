using Microsoft.EntityFrameworkCore;
using Moda.Common.Domain.Models;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommand(Guid ConnectionId, IntegrationRegistration<Guid, Guid> IntegrationRegistration) : ICommand;

internal sealed class UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommandHandler : ICommandHandler<UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommand>
{
    private const string AppRequestName = nameof(UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommand);

    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommandHandler> _logger;

    public UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeProvider, ILogger<UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateAzureDevOpsBoardsWorkspaceIntegrationStateCommand request, CancellationToken cancellationToken)
    {
        var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections.FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            _logger.LogError("Unable to find Azure DevOps Boards connection with id {ConnectionId}.", request.ConnectionId);
            return Result.Failure($"Unable to find Azure DevOps Boards connection with id {request.ConnectionId}.");
        }

        var updateWorkspaceResult = connection.UpdateWorkspaceIntegrationState(request.IntegrationRegistration, _dateTimeProvider.Now);
        if (updateWorkspaceResult.IsFailure)
        {
            _logger.LogError("Errors occurred while processing {AppRequestName}. {Error}", AppRequestName, updateWorkspaceResult.Error);

            return Result.Failure($"Errors occurred while processing {AppRequestName}.");
        }

        await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
