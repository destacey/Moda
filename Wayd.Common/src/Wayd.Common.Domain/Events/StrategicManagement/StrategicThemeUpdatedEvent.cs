using Wayd.Common.Domain.Enums.StrategicManagement;
using Wayd.Common.Domain.Interfaces.StrategicManagement;
using NodaTime;

namespace Wayd.Common.Domain.Events.StrategicManagement;

public sealed record StrategicThemeUpdatedEvent : DomainEvent
{
    public StrategicThemeUpdatedEvent(IStrategicThemeData strategicTheme, Instant timestamp)
    {
        Id = strategicTheme.Id;
        Name = strategicTheme.Name;
        Description = strategicTheme.Description;
        State = strategicTheme.State;

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public StrategicThemeState State { get; }
}
