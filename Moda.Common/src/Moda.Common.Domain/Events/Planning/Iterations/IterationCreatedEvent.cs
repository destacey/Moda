using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Domain.Interfaces.Planning.Iterations;
using Moda.Common.Domain.Models.Planning.Iterations;
using NodaTime;

namespace Moda.Common.Domain.Events.Planning.Iterations;
public sealed record IterationCreatedEvent : DomainEvent, ISimpleIteration
{
    public IterationCreatedEvent(ISimpleIteration iteration, Instant timestamp)
    {
        Id = iteration.Id;
        Key = iteration.Key;
        Name = iteration.Name;
        Type = iteration.Type;
        State = iteration.State;
        DateRange = iteration.DateRange;
        TeamId = iteration.TeamId;

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public int Key { get; }
    public string Name { get; }
    public IterationType Type { get; }
    public IterationState State { get; }
    public IterationDateRange DateRange { get; }
    public Guid? TeamId { get; }
}
