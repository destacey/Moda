using Moda.Common.Application.Events;
using Moda.Common.Domain.Events;
using Moda.Organization.Domain.Models;

namespace Moda.Planning.Application.PlanningTeams.EventHandlers;
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

    private async Task UpsertPlanningTeam(BaseTeam baseTeam, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == baseTeam.Id, cancellationToken);
        string action = existingTeam is null ? "created" : "updated";

        try
        {
            Guid teamId = default!;
            string teamName = default!;

            if (existingTeam is null)
            {
                var planningTeam = new PlanningTeam(baseTeam);
                await _planningDbContext.PlanningTeams.AddAsync(planningTeam);

                teamId = planningTeam.Id;
                teamName = planningTeam.Name;
            }
            else
            {
                existingTeam.Update(baseTeam);

                teamId = existingTeam.Id;
                teamName = existingTeam.Name;
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Planning Team {Action}. {PlanningTeamId} - {PlanningTeamName}", action, teamId, teamName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Planning Team {Action} action failed to save. {PlanningTeamId} - {PlanningTeamName}", action, baseTeam.Id, baseTeam.Name);
        }
    }
}
