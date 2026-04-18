using Wayd.Common.Domain.Enums.StrategicManagement;
using Wayd.Common.Domain.Interfaces.StrategicManagement;
using NodaTime;

namespace Wayd.Common.Domain.Events.StrategicManagement;

public sealed record StrategicThemeCreatedEvent : DomainEvent, IStrategicThemeData
{
    public StrategicThemeCreatedEvent(IStrategicThemeData strategicTheme, Instant timestamp)
    {
        Id = strategicTheme.Id;
        Key = strategicTheme.Key;
        Name = strategicTheme.Name;
        Description = strategicTheme.Description;
        State = strategicTheme.State;

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public int Key { get; }
    public string Name { get; }
    public string Description { get; }
    public StrategicThemeState State { get; }
}
