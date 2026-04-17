using Wayd.Common.Domain.Enums;
using Wayd.Common.Domain.Models;
using NodaTime;

namespace Wayd.Common.Domain.Events;

public record IntegrationStateChangedEvent<TId> : DomainEvent
{
    public IntegrationStateChangedEvent(SystemContext systemContext, IntegrationState<TId> integrationState, Instant timestamp)
    {
        SystemContext = systemContext;
        IntegrationState = integrationState;
        Timestamp = timestamp;
    }

    public SystemContext SystemContext { get; }
    public IntegrationState<TId> IntegrationState { get; }
}
