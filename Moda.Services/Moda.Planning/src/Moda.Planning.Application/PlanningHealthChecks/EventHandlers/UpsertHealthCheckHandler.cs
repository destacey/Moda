using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events;
using Moda.Health.Models;

namespace Moda.Planning.Application.PlanningHealthChecks.EventHandlers;

// TODO - putting a dependency on the Health project is not good.  This should be a named event rather a generic event of T.  Need to refactor this.
internal sealed class UpsertHealthCheckHandler :
    IEventNotificationHandler<EntityCreatedEvent<HealthCheck>>,
    IEventNotificationHandler<EntityUpdatedEvent<HealthCheck>>,
    IEventNotificationHandler<EntityDeletedEvent<HealthCheck>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<UpsertHealthCheckHandler> _logger;

    public UpsertHealthCheckHandler(IPlanningDbContext planningDbContext, ILogger<UpsertHealthCheckHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task Handle(EventNotification<EntityCreatedEvent<HealthCheck>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningHealthCheck(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityUpdatedEvent<HealthCheck>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningHealthCheck(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeletedEvent<HealthCheck>> notification, CancellationToken cancellationToken)
    {
        await DeletePlanningHealthCheck(notification.Event.Entity, cancellationToken);
    }

    /// <summary>
    /// Persist the last health check for the given planning object.
    /// </summary>
    /// <param name="healthCheck"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpsertPlanningHealthCheck(HealthCheck healthCheck, CancellationToken cancellationToken)
    {
        if (healthCheck.Context is not SystemContext.PlanningProgramIncrementObjective)
            return;

        try
        {
            var planningHealthCheck = await _planningDbContext.PlanningHealthChecks
                .FirstOrDefaultAsync(x => x.ObjectId == healthCheck.ObjectId, cancellationToken);

            if (planningHealthCheck is null)
            {
                planningHealthCheck = new SimpleHealthCheck(healthCheck.ObjectId, healthCheck.Id, healthCheck.Status, healthCheck.ReportedOn, healthCheck.Expiration);
                await _planningDbContext.PlanningHealthChecks.AddAsync(planningHealthCheck, cancellationToken);
            }
            else if (healthCheck.ReportedOn < planningHealthCheck.ReportedOn)
            {
                // this happens when a new one is created and previous one has its expiration updated
                // we only want the latest one
                return;
            }
            else
            {
                planningHealthCheck.Update(healthCheck.Id, healthCheck.Status, healthCheck.ReportedOn, healthCheck.Expiration);
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[{SystemActionType}] Upserted planning health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Error upserting planning health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
    }

    private async Task DeletePlanningHealthCheck(HealthCheck healthCheck, CancellationToken cancellationToken)
    {
        if (healthCheck.Context is not SystemContext.PlanningProgramIncrementObjective)
            return;

        try
        {
            var planningHealthCheck = await _planningDbContext.PlanningHealthChecks
            .FirstOrDefaultAsync(x => x.ObjectId == healthCheck.ObjectId, cancellationToken);

            if (planningHealthCheck is null)
                return;

            _planningDbContext.PlanningHealthChecks.Remove(planningHealthCheck);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[{SystemActionType}] Deleted planning health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Error deleting planning health check {HealthCheckId} for object {ObjectId}", SystemActionType.ServiceDataReplication, healthCheck.Id, healthCheck.ObjectId);
        }
    }
}
