using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events.StrategicManagement;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.StrategicThemes.EventHandlers;
internal sealed class StrategicThemeChangedHandler(IProjectPortfolioManagementDbContext ppmContext, ILogger<StrategicThemeChangedHandler> logger) :
    IEventNotificationHandler<StrategicThemeCreatedEvent>,
    IEventNotificationHandler<StrategicThemeUpdatedEvent>,
    IEventNotificationHandler<StrategicThemeDeletedEvent>
{
    private readonly IProjectPortfolioManagementDbContext _ppmContext = ppmContext;
    private readonly ILogger<StrategicThemeChangedHandler> _logger = logger;

    public async Task Handle(EventNotification<StrategicThemeCreatedEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling PPM {SystemActionType} for a new Strategic Theme {StrategicThemeId}.", SystemActionType.ServiceDataReplication, notification.Event.Id);
        await CreateStrategicTheme(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<StrategicThemeUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling PPM {SystemActionType} for an updated Strategic Theme {StrategicThemeId}.", SystemActionType.ServiceDataReplication, notification.Event.Id);
        await UpdateStrategicTheme(notification.Event, cancellationToken);
    }

    public async Task Handle(EventNotification<StrategicThemeDeletedEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling PPM {SystemActionType} for a deleted Strategic Theme {StrategicThemeId}.", SystemActionType.ServiceDataReplication, notification.Event.Id);
        await DeleteStrategicTheme(notification.Event, cancellationToken);
    }

    private async Task CreateStrategicTheme(StrategicThemeCreatedEvent createdEvent, CancellationToken cancellationToken)
    {
        try
        {
            if (await _ppmContext.PpmStrategicThemes.AnyAsync(x => x.Id == createdEvent.Id, cancellationToken))
            {
                _logger.LogCritical("Error processing PPM {SystemActionType} for a new Strategic Theme. Strategic Theme {StrategicThemeId} already exists in the PPM system.", SystemActionType.ServiceDataReplication, createdEvent.Id);
                return;
            }

            var theme = new StrategicTheme(createdEvent);
            await _ppmContext.PpmStrategicThemes.AddAsync(theme, cancellationToken);

            await _ppmContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successful PPM {SystemActionType} for the Strategic Theme {StrategicThemeId} created action.", SystemActionType.ServiceDataReplication, createdEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception handling PPM {SystemActionType} for the Stategic Theme {StrategicThemeId} created action.", SystemActionType.ServiceDataReplication, createdEvent.Id);
        }
    }

    private async Task UpdateStrategicTheme(StrategicThemeUpdatedEvent updatedEvent, CancellationToken cancellationToken)
    {
        try
        {
            var existingStrategicTheme = await _ppmContext.PpmStrategicThemes
                .FirstOrDefaultAsync(x => x.Id == updatedEvent.Id, cancellationToken);
            if (existingStrategicTheme is null)
            {
                _logger.LogCritical("Error processing PPM {SystemActionType} for an existing Strategic Theme. Strategic Theme {StrategicThemeId} does not exist in the PPM system.", SystemActionType.ServiceDataReplication, updatedEvent.Id);
                return;
            }

            existingStrategicTheme.Update(updatedEvent.Name, updatedEvent.Description, updatedEvent.State);


            await _ppmContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successful PPM {SystemActionType} for the Strategic Theme {StrategicThemeId} update action.", SystemActionType.ServiceDataReplication, updatedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception handling PPM {SystemActionType} for the Stategic Theme {StrategicThemeId} update action.", SystemActionType.ServiceDataReplication, updatedEvent.Id);
        }
    }

    private async Task DeleteStrategicTheme(StrategicThemeDeletedEvent deletedEvent, CancellationToken cancellationToken)
    {
        try
        {
            var existingStrategicTheme = await _ppmContext.PpmStrategicThemes
                .FirstOrDefaultAsync(x => x.Id == deletedEvent.Id, cancellationToken);
            if (existingStrategicTheme is null)
            {
                _logger.LogWarning("Error processing PPM {SystemActionType} for a deleted Strategic Theme. Strategic Theme {StrategicThemeId} does not exist in the PPM system.", SystemActionType.ServiceDataReplication, deletedEvent.Id);
                return;
            }

            _ppmContext.PpmStrategicThemes.Remove(existingStrategicTheme);

            await _ppmContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successful PPM {SystemActionType} for the Strategic Theme {StrategicThemeId} delete action.", SystemActionType.ServiceDataReplication, deletedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception handling PPM {SystemActionType} for the Stategic Theme {StrategicThemeId} delete action.", SystemActionType.ServiceDataReplication, deletedEvent.Id);
        }
    }
}
