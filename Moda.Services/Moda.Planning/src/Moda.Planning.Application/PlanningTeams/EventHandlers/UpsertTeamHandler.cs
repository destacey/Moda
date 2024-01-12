using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events;
using Moda.Common.Domain.Interfaces.Organization;
using Moda.Organization.Domain.Models;

namespace Moda.Planning.Application.PlanningTeams.EventHandlers;

// TODO - putting a dependency on the Organization project is not good.  This should be a named event rather a generic event of T.  Need to refactor this.
internal sealed class UpsertTeamHandler :
    IEventNotificationHandler<EntityCreatedEvent<Team>>,
    IEventNotificationHandler<EntityUpdatedEvent<Team>>,
    IEventNotificationHandler<EntityActivatedEvent<Team>>,
    IEventNotificationHandler<EntityDeactivatedEvent<Team>>,
    IEventNotificationHandler<EntityDeletedEvent<Team>>,
    IEventNotificationHandler<EntityCreatedEvent<TeamOfTeams>>,
    IEventNotificationHandler<EntityUpdatedEvent<TeamOfTeams>>,
    IEventNotificationHandler<EntityActivatedEvent<TeamOfTeams>>,
    IEventNotificationHandler<EntityDeactivatedEvent<TeamOfTeams>>,
    IEventNotificationHandler<EntityDeletedEvent<TeamOfTeams>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<UpsertTeamHandler> _logger;

    public UpsertTeamHandler(IPlanningDbContext planningDbContext, ILogger<UpsertTeamHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task Handle(EventNotification<EntityCreatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityUpdatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityActivatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeactivatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeletedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityCreatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityUpdatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityActivatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeactivatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeletedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertPlanningTeam(notification.Event.Entity, cancellationToken);
    }

    private async Task UpsertPlanningTeam(ISimpleTeam simpleTeam, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == simpleTeam.Id, cancellationToken);
        string action = existingTeam is null ? "created" : "updated";

        try
        {
            Guid teamId = default!;
            string teamName = default!;

            if (existingTeam is null)
            {
                var planningTeam = new PlanningTeam(simpleTeam);
                await _planningDbContext.PlanningTeams.AddAsync(planningTeam);

                teamId = planningTeam.Id;
                teamName = planningTeam.Name;
            }
            else
            {
                existingTeam.Update(simpleTeam);

                teamId = existingTeam.Id;
                teamName = existingTeam.Name;
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[{SystemActionType}] Planning Team {Action}. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, action, teamId, teamName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team {Action} action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, action, simpleTeam.Id, simpleTeam.Name);
        }
    }
}
