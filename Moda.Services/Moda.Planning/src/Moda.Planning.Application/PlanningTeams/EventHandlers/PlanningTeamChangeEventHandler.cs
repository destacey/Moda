using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events.Organization;

namespace Moda.Planning.Application.PlanningTeams.EventHandlers;

internal sealed class PlanningTeamChangeEventHandler :
    IEventNotificationHandler<TeamCreatedEvent>,
    IEventNotificationHandler<TeamUpdatedEvent>,
    IEventNotificationHandler<TeamActivatedEvent>,
    IEventNotificationHandler<TeamDeactivatedEvent>,
    IEventNotificationHandler<TeamDeletedEvent>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<PlanningTeamChangeEventHandler> _logger;

    public PlanningTeamChangeEventHandler(IPlanningDbContext planningDbContext, ILogger<PlanningTeamChangeEventHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task Handle(EventNotification<TeamCreatedEvent> notification, CancellationToken cancellationToken)
    {
        await CreatePlanningTeam(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<TeamUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        await UpdatePlanningTeam(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<TeamActivatedEvent> notification, CancellationToken cancellationToken)
    {
        await ActivatePlanningTeam(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<TeamDeactivatedEvent> notification, CancellationToken cancellationToken)
    {
        await DeactivatePlanningTeam(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<TeamDeletedEvent> notification, CancellationToken cancellationToken)
    {
        await DeletePlanningTeam(notification.Event, cancellationToken);
    }

    private async Task CreatePlanningTeam(TeamCreatedEvent team, CancellationToken cancellationToken)
    {
        if (await _planningDbContext.PlanningTeams.AnyAsync(t => t.Id == team.Id, cancellationToken))
        {
            _logger.LogInformation("[{SystemActionType}] Planning Team create action skipped. Team already exists. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, team.Name);
            return;
        }

        var planningTeam = new PlanningTeam(team);
        try
        {
            await _planningDbContext.PlanningTeams.AddAsync(planningTeam, cancellationToken);
            await _planningDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[{SystemActionType}] Planning Team created. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, team.Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team create action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, team.Name);
        }
    }

    private async Task UpdatePlanningTeam(TeamUpdatedEvent team, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);
        if (existingTeam is null)
        {
            _logger.LogInformation("[{SystemActionType}] Planning Team update action skipped. Unable to find work team {PlanningTeamId} to update.", SystemActionType.ServiceDataReplication, team.Id);
            return;
        }
        try
        {
            existingTeam.Update(team);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[{SystemActionType}] Planning Team updated. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, team.Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team update action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, team.Name);
        }
    }

    private async Task ActivatePlanningTeam(TeamActivatedEvent team, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);
        if (existingTeam is null)
        {
            _logger.LogInformation("[{SystemActionType}] Planning Team activate action skipped. Unable to find work team {PlanningTeamId} to activate.", SystemActionType.ServiceDataReplication, team.Id);
            return;
        }
        try
        {
            existingTeam.UpdateIsActive(true);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[{SystemActionType}] Planning Team activated. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, existingTeam.Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team activate action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, existingTeam.Name);
        }
    }

    private async Task DeactivatePlanningTeam(TeamDeactivatedEvent team, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);
        if (existingTeam is null)
        {
            _logger.LogInformation("[{SystemActionType}] Planning Team deactivate action skipped. Unable to find work team {PlanningTeamId} to deactivate.", SystemActionType.ServiceDataReplication, team.Id);
            return;
        }
        try
        {
            existingTeam.UpdateIsActive(false);
            await _planningDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[{SystemActionType}] Planning Team deactivated. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, existingTeam.Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team deactivate action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, existingTeam.Name);
        }
    }

    private async Task DeletePlanningTeam(TeamDeletedEvent team, CancellationToken cancellationToken)
    {
        var existingTeam = await _planningDbContext.PlanningTeams.FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);

        try
        {
            if (existingTeam is null)
            {
                _logger.LogInformation("[{SystemActionType}] Planning Team deleted. Unable to find work team {PlanningTeamId} to delete.", SystemActionType.ServiceDataReplication, team.Id);
            }
            else
            {
                // TODO: consider making the team inactive or archiving it instead of deleting it.  Maybe we only delete if the planning team has never been used?

                _planningDbContext.PlanningTeams.Remove(existingTeam);
                await _planningDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("[{SystemActionType}] Planning Team deleted. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, existingTeam.Id, existingTeam.Name);
            }

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{SystemActionType}] Planning Team delete action failed to save. {PlanningTeamId} - {PlanningTeamName}", SystemActionType.ServiceDataReplication, team.Id, existingTeam?.Name ?? "Unknown Team");
        }
    }
}
