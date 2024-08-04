using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events;
using Moda.Common.Domain.Interfaces.Organization;
using Moda.Organization.Domain.Models;

namespace Moda.Work.Application.WorkTeams.EventHandlers;

// TODO - putting a dependency on the Organization project is not good.  This should be a named event rather a generic event of T.  Need to refactor this.
internal sealed class WorkTeamChangeEventHandler :
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
    private readonly IWorkDbContext _workDbContext;
    private readonly ILogger<WorkTeamChangeEventHandler> _logger;

    public WorkTeamChangeEventHandler(IWorkDbContext workDbContext, ILogger<WorkTeamChangeEventHandler> logger)
    {
        _workDbContext = workDbContext;
        _logger = logger;
    }

    public async Task Handle(EventNotification<EntityCreatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityUpdatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityActivatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeactivatedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeletedEvent<Team>> notification, CancellationToken cancellationToken)
    {
        await DeleteWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityCreatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityUpdatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityActivatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeactivatedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await UpsertWorkTeam(notification.Event.Entity, cancellationToken);
    }

    public async Task Handle(EventNotification<EntityDeletedEvent<TeamOfTeams>> notification, CancellationToken cancellationToken)
    {
        await DeleteWorkTeam(notification.Event.Entity, cancellationToken);
    }

    private async Task UpsertWorkTeam(ISimpleTeam simpleTeam, CancellationToken cancellationToken)
    {
        var existingTeam = await _workDbContext.WorkTeams.FirstOrDefaultAsync(t => t.Id == simpleTeam.Id, cancellationToken);
        string action = existingTeam is null ? "created" : "updated";

        try
        {
            Guid teamId = default!;
            string teamName = default!;

            if (existingTeam is null)
            {
                var planningTeam = new WorkTeam(simpleTeam);
                await _workDbContext.WorkTeams.AddAsync(planningTeam, cancellationToken);

                teamId = planningTeam.Id;
                teamName = planningTeam.Name;
            }
            else
            {
                existingTeam.Update(simpleTeam);

                teamId = existingTeam.Id;
                teamName = existingTeam.Name;
            }

            await _workDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[{SystemActionType}] Work Team {Action}. {WorkTeamId} - {WorkTeamName}", SystemActionType.ServiceDataReplication, action, teamId, teamName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Work Team {Action} action failed to save. {WorkTeamId} - {WorkTeamName}", SystemActionType.ServiceDataReplication, action, simpleTeam.Id, simpleTeam.Name);
        }
    }

    private async Task DeleteWorkTeam(ISimpleTeam simpleTeam, CancellationToken cancellationToken)
    {
        var existingTeam = await _workDbContext.WorkTeams.FirstOrDefaultAsync(t => t.Id == simpleTeam.Id, cancellationToken);
        string action = "deleted";

        try
        {
            if (existingTeam is null)
            {
                _logger.LogInformation("[{SystemActionType}] Work Team {Action}. Unable to find work team {WorkTeamId} to delete.", SystemActionType.ServiceDataReplication, action, simpleTeam.Id);
            }
            else
            {
                _workDbContext.WorkTeams.Remove(existingTeam);
                await _workDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("[{SystemActionType}] Work Team {Action}. {WorkTeamId} - {WorkTeamName}", SystemActionType.ServiceDataReplication, action, existingTeam.Id, existingTeam.Name);
            }

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Work Team {Action} action failed to save. {WorkTeamId} - {WorkTeamName}", SystemActionType.ServiceDataReplication, action, simpleTeam.Id, simpleTeam.Name);
        }
    }
}
