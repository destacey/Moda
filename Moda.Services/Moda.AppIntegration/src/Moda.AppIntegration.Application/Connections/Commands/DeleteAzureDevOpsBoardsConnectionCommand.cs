using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Connections.Commands;

public sealed record DeleteAzureDevOpsBoardsConnectionCommand(Guid Id) : ICommand;

internal sealed class DeleteAzureDevOpsBoardsConnectionCommandHandler : ICommandHandler<DeleteAzureDevOpsBoardsConnectionCommand>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<DeleteAzureDevOpsBoardsConnectionCommandHandler> _logger;

    public DeleteAzureDevOpsBoardsConnectionCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<DeleteAzureDevOpsBoardsConnectionCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAzureDevOpsBoardsConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (connection is null)
            {
                _logger.LogError("Azure DevOps Boards Connection {ConnectionId} not found.", request.Id);
                return Result.Failure($"Azure DevOps Boards Connection {request.Id} not found.");
            }

            _appIntegrationDbContext.AzureDevOpsBoardsConnections.Remove(connection);
            await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Azure DevOps Boards Connection {ConnectionId}.", request.Id);
            return Result.Failure($"Error deleting Azure DevOps Boards Connection {request.Id}. {ex.Message}");
        }
    }
}
