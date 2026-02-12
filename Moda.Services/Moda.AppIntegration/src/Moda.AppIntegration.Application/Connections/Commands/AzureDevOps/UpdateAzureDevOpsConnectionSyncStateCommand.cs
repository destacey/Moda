using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Connections.Commands.AzureDevOps;
public sealed record UpdateAzureDevOpsConnectionSyncStateCommand(Guid Id, bool IsSyncEnabled) : ICommand;

internal sealed class UpdateAzureDevOpsConnectionSyncStateCommandHandler(ILogger<UpdateAzureDevOpsConnectionSyncStateCommandHandler> logger, IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateAzureDevOpsConnectionSyncStateCommand>
{
    private const string AppRequestName = nameof(UpdateAzureDevOpsWorkspaceIntegrationStateCommand);

    private readonly ILogger<UpdateAzureDevOpsConnectionSyncStateCommandHandler> _logger = logger;
    private readonly IAppIntegrationDbContext _appIntegrationDbContext = appIntegrationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result> Handle(UpdateAzureDevOpsConnectionSyncStateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (connection is null)
                return Result.Failure<Guid>("Azure DevOps connection not found.");

            var updateResult = connection.SetSyncState(request.IsSyncEnabled, _dateTimeProvider.Now);
            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _appIntegrationDbContext.Entry(connection).ReloadAsync(cancellationToken);
                connection.ClearDomainEvents();

                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", AppRequestName, request, updateResult.Error);
                return Result.Failure<Guid>(updateResult.Error);
            }

            await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(connection.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", AppRequestName, request);

            return Result.Failure($"Moda Request: Exception for Request AppRequestName {request}");
        }
    }
}