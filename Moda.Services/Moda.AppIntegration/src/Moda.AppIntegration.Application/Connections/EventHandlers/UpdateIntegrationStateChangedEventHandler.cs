using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Events;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Events;
using Serilog.Context;

namespace Moda.AppIntegration.Application.Connections.EventHandlers;
internal sealed class UpdateIntegrationStateChangedEventHandler : IEventNotificationHandler<IntegrationStateChangedEvent<Guid>>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<UpdateIntegrationStateChangedEventHandler> _logger;

    public UpdateIntegrationStateChangedEventHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<UpdateIntegrationStateChangedEventHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task Handle(EventNotification<IntegrationStateChangedEvent<Guid>> notification, CancellationToken cancellationToken)
    {
        if (notification.Event.SystemContext == SystemContext.WorkWorkProcess)
        {
            var connections = await _appIntegrationDbContext.AzureDevOpsBoardsConnections.ToListAsync(cancellationToken);
            foreach (var connection in connections)
            {
                var workProcess = connection.Configuration.WorkProcesses.FirstOrDefault(p => p.HasIntegration && p.IntegrationState!.InternalId == notification.Event.IntegrationState.InternalId);
                if (workProcess is not null)
                {
                    workProcess.UpdateIntegrationState(notification.Event.IntegrationState.IsActive);

                    using (LogContext.PushProperty("EventPayload", notification.Event))
                    {
                        _logger.LogInformation("Event processed for {EventHandler}", nameof(UpdateIntegrationStateChangedEventHandler));
                    }

                    break;
                }
            }
        }
    }
}
