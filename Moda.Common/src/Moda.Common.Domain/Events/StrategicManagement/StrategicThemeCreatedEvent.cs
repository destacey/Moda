using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.Common.Domain.Interfaces.StrategicManagement;
using NodaTime;

namespace Moda.Common.Domain.Events.StrategicManagement;
public record StrategicThemeCreatedEvent : DomainEvent
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

    public Guid Id { get; init; }
    public int Key { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public StrategicThemeState State { get; init; }
}
