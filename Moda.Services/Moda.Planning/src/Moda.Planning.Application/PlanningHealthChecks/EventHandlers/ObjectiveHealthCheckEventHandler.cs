using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events.Health;

namespace Moda.Planning.Application.PlanningHealthChecks.EventHandlers;

internal sealed class ObjectiveHealthCheckEventHandler(IPlanningDbContext planningDbContext, ILogger<ObjectiveHealthCheckEventHandler> logger) :
    IEventNotificationHandler<HealthCheckCreatedEvent>,
    IEventNotificationHandler<HealthCheckUpdatedEvent>,
    IEventNotificationHandler<HealthCheckDeletedEvent>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly ILogger<ObjectiveHealthCheckEventHandler> _logger = logger;

    public async Task Handle(EventNotification<HealthCheckCreatedEvent> notification, CancellationToken cancellationToken)
    {
        await CreatePiObjectiveHealthCheck(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<HealthCheckUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        await UpdatePiObjectiveHealthCheck(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<HealthCheckDeletedEvent> notification, CancellationToken cancellationToken)
    {
        await DeletePiObjectiveHealthCheck(notification.Event, cancellationToken);
    }

    /// <summary>
    /// Creates a health check record for a Planning Interval (PI) objective if the provided event is relevant and more
    /// recent than any existing health check.
    /// </summary>
    /// <remarks>If a more recent health check already exists for the specified objective, the method updates
    /// it; otherwise, it creates a new health check. If the event is for an older or duplicate health check, no changes
    /// are made. Logging is performed for both successful and failed operations.</remarks>
    /// <param name="healthCheck">The event data containing information about the health check to be created. Must have a context of
    /// PlanningPlanningIntervalObjective.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the health check creation process has
    /// finished.</returns>
    private async Task CreatePiObjectiveHealthCheck(HealthCheckCreatedEvent healthCheck, CancellationToken cancellationToken)
    {
        if (healthCheck.Context is not SystemContext.PlanningPlanningIntervalObjective)
            return;

        try
        {
            var objective = await GetObjective(healthCheck.ObjectId, cancellationToken);
            if (objective is null)
                return;

            var currentObjectiveHealthCheck = objective.HealthCheck;
            if (currentObjectiveHealthCheck is not null)
            {
                if (healthCheck.Id == currentObjectiveHealthCheck.Id)
                {
                    // already exists
                    return;
                }
                else if (healthCheck.ReportedOn <= currentObjectiveHealthCheck.ReportedOn)
                {
                    // this event is for an older health check, ignore
                    return;
                }
                else
                {
                    var removeResult = objective.RemoveHealthCheck();
                    if (removeResult.IsFailure)
                    {
                        _logger.LogWarning("[{SystemActionType}] Failed to remove existing health check for objective {ObjectiveId}: {Error}", SystemActionType.ServiceDataReplication, healthCheck.ObjectId, removeResult.Error);
                        return;
                    }

                    _planningDbContext.PlanningHealthChecks.Remove(currentObjectiveHealthCheck);
                    await _planningDbContext.SaveChangesAsync(cancellationToken);
                }
            }

            var planningHealthCheck = new SimpleHealthCheck(healthCheck.ObjectId, healthCheck.Id, healthCheck.Status, healthCheck.ReportedOn, healthCheck.Expiration);

            await _planningDbContext.PlanningHealthChecks.AddAsync(planningHealthCheck, cancellationToken);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[{SystemActionType}] Created PI objective health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Error creating PI objective health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
    }

    /// <summary>
    /// Updates the health check status for a Planning Interval Objective if the specified event applies to it.
    /// </summary>
    /// <remarks>If the health check does not match the objective or the event context is not
    /// PlanningPlanningIntervalObjective, no update is performed. Logging is used to record failures and successful
    /// updates.</remarks>
    /// <param name="healthCheck">The event containing updated health check information for the Planning Interval Objective. Must have a context
    /// of PlanningPlanningIntervalObjective.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    private async Task UpdatePiObjectiveHealthCheck(HealthCheckUpdatedEvent healthCheck, CancellationToken cancellationToken)
    {
        if (healthCheck.Context is not SystemContext.PlanningPlanningIntervalObjective)
            return;
        try
        {
            var objective = await GetObjective(healthCheck.ObjectId, cancellationToken);
            if (objective is null || objective.HealthCheck is null)
                return;

            if (objective.HealthCheck.Id != healthCheck.Id)
                return;

            var updateResult = objective.HealthCheck.Update(healthCheck.Status, healthCheck.Expiration);
            if (updateResult.IsFailure)
            {
                _logger.LogWarning("[{SystemActionType}] Failed to update existing health check for objective {ObjectiveId}: {Error}", SystemActionType.ServiceDataReplication, healthCheck.ObjectId, updateResult.Error);
                return;
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[{SystemActionType}] Updated PI objective health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Error updating PI objective health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
    }

    /// <summary>
    /// Deletes the health check associated with a Planning Interval Objective if the specified event matches the
    /// expected context and identifiers.
    /// </summary>
    /// <remarks>If the health check or objective does not exist, or if the identifiers do not match, no
    /// action is taken. Logging is performed for both successful and failed delete attempts. This method does not throw
    /// on failure; errors are logged instead.</remarks>
    /// <param name="healthCheck">The event data representing the health check to be deleted. Must have a context of
    /// PlanningPlanningIntervalObjective and a valid object and health check identifier.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    private async Task DeletePiObjectiveHealthCheck(HealthCheckDeletedEvent healthCheck, CancellationToken cancellationToken)
    {
        if (healthCheck.Context is not SystemContext.PlanningPlanningIntervalObjective)
            return;

        try
        {
            var objective = await GetObjective(healthCheck.ObjectId, cancellationToken);
            if (objective is null || objective.HealthCheck is null)
                return;

            if (objective.HealthCheck.Id != healthCheck.Id)
                return;

            var currentObjectiveHealthCheck = objective.HealthCheck;
            var removeResult = objective.RemoveHealthCheck();
            if (removeResult.IsFailure)
            {
                _logger.LogWarning("[{SystemActionType}] Failed to remove existing health check for objective {ObjectiveId}: {Error}", SystemActionType.ServiceDataReplication, healthCheck.ObjectId, removeResult.Error);
                return;
            }

            _planningDbContext.PlanningHealthChecks.Remove(currentObjectiveHealthCheck);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[{SystemActionType}] Deleted PI objective health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Error deleting PI objective health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
    }

    private async Task<PlanningIntervalObjective?> GetObjective(Guid objectiveId, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervalObjectives
            .Include(x => x.HealthCheck)
            .FirstOrDefaultAsync(x => x.Id == objectiveId, cancellationToken);
    }
}
