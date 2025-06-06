﻿using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events;
using Moda.Common.Domain.Interfaces.Organization;
using Moda.Organization.Domain.Models;

namespace Moda.Planning.Application.PlanningTeams.EventHandlers;

// TODO - putting a dependency on the Organization project is not good.  This should be a named event rather a generic event of T.  Need to refactor this.
internal sealed class PlanningTeamChangeEventHandler :
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
    private readonly ILogger<PlanningTeamChangeEventHandler> _logger;

    public PlanningTeamChangeEventHandler(IPlanningDbContext planningDbContext, ILogger<PlanningTeamChangeEventHandler> logger)
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
        await DeletePlanningTeam(notification.Event.Entity, cancellationToken);
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
        await DeletePlanningTeam(notification.Event.Entity, cancellationToken);
    }

    private async Task UpsertPlanningTeam(ISimpleTeam simpleTeam, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == simpleTeam.Id, cancellationToken);
        string action = existingTeam is null ? "created" : "updated";

        try
        {
            if (existingTeam is null)
            {
                var planningTeam = new PlanningTeam(simpleTeam);
                await _planningDbContext.PlanningTeams.AddAsync(planningTeam, cancellationToken);
            }
            else
            {
                existingTeam.Update(simpleTeam);
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[{SystemActionType}] Planning Team {Action}. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, action, simpleTeam.Id, simpleTeam.Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team {Action} action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, action, simpleTeam.Id, simpleTeam.Name);
        }
    }

    private async Task DeletePlanningTeam(ISimpleTeam simpleTeam, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == simpleTeam.Id, cancellationToken);
        string action = "deleted";

        try
        {
            if (existingTeam is null)
            {
                _logger.LogInformation("[{SystemActionType}] Planning Team {Action}. Unable to find work team {PlanningTeamId} to delete.", SystemActionType.ServiceDataReplication, action, simpleTeam.Id);
            }
            else
            {
                _planningDbContext.PlanningTeams.Remove(existingTeam);
                await _planningDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("[{SystemActionType}] Planning Team {Action}. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, action, existingTeam.Id, existingTeam.Name);
            }

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team {Action} action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, action, simpleTeam.Id, simpleTeam.Name);
        }
    }
}
