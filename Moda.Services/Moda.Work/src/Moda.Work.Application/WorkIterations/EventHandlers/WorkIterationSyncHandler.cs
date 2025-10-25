using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events.Planning.Iterations;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.WorkIterations.EventHandlers;
internal sealed class WorkIterationSyncHandler(IWorkDbContext workDbContext, ILogger<WorkIterationSyncHandler> logger) :
    IEventNotificationHandler<IterationCreatedEvent>,
    IEventNotificationHandler<IterationUpdatedEvent>,
    IEventNotificationHandler<IterationDeletedEvent>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<WorkIterationSyncHandler> _logger = logger;

    public async Task Handle(EventNotification<IterationCreatedEvent> notification, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Handling Work {SystemActionType} for a new Iteration {IterationId}.", SystemActionType.ServiceDataReplication, notification.Event.Id);
        await CreateIteration(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<IterationUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Handling Work {SystemActionType} for an updated Iteration {IterationId}.", SystemActionType.ServiceDataReplication, notification.Event.Id);
        await UpdateIteration(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<IterationDeletedEvent> notification, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Handling Work {SystemActionType} for a deleted Iteration {IterationId}.", SystemActionType.ServiceDataReplication, notification.Event.Id);
        await DeleteIteration(notification.Event, cancellationToken);
    }

    private async Task CreateIteration(IterationCreatedEvent createdEvent, CancellationToken cancellationToken)
    {
        try
        {
            if (await _workDbContext.WorkIterations.AnyAsync(x => x.Id == createdEvent.Id, cancellationToken))
            {
                _logger.LogCritical("Error processing Work {SystemActionType} for a new Iteration. Iteration {IterationId} already exists in the Work system.", SystemActionType.ServiceDataReplication, createdEvent.Id);
                return;
            }

            var iteration = new WorkIteration(createdEvent);

            await _workDbContext.WorkIterations.AddAsync(iteration, cancellationToken);
            await _workDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successful Work {SystemActionType} for the Iteration {IterationId} created action.", SystemActionType.ServiceDataReplication, createdEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception handling Work {SystemActionType} for the Iteration {IterationId} created action.", SystemActionType.ServiceDataReplication, createdEvent.Id);
        }
    }

    private async Task UpdateIteration(IterationUpdatedEvent updatedEvent, CancellationToken cancellationToken)
    {
        try
        {
            var iteration = await _workDbContext.WorkIterations.FirstOrDefaultAsync(x => x.Id == updatedEvent.Id, cancellationToken);
            if (iteration == null)
            {
                _logger.LogCritical("Error processing Work {SystemActionType} for an updated Iteration. Iteration {IterationId} does not exist in the Work system.", SystemActionType.ServiceDataReplication, updatedEvent.Id);
                return;
            }

            iteration.Update(updatedEvent);

            await _workDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successful Work {SystemActionType} for the Iteration {IterationId} updated action.", SystemActionType.ServiceDataReplication, updatedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception handling Work {SystemActionType} for the Iteration {IterationId} updated action.", SystemActionType.ServiceDataReplication, updatedEvent.Id);
        }
    }

    private async Task DeleteIteration(IterationDeletedEvent deletedEvent, CancellationToken cancellationToken)
    {
        try
        {
            var iteration = await _workDbContext.WorkIterations.FirstOrDefaultAsync(x => x.Id == deletedEvent.Id, cancellationToken);
            if (iteration == null)
            {
                _logger.LogCritical("Error processing Work {SystemActionType} for a deleted Iteration. Iteration {IterationId} does not exist in the Work system.", SystemActionType.ServiceDataReplication, deletedEvent.Id);
                return;
            }

            _workDbContext.WorkIterations.Remove(iteration);

            await _workDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successful Work {SystemActionType} for the Iteration {IterationId} deleted action.", SystemActionType.ServiceDataReplication, deletedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception handling Work {SystemActionType} for the Iteration {IterationId} deleted action.", SystemActionType.ServiceDataReplication, deletedEvent.Id);
        }
    }

}
