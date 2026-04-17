using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Models;
using Wayd.Common.Domain.Enums.Planning;
using NodaTime;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoIteration : IExternalIteration<AzdoIterationMetadata>
{
    public AzdoIteration(int id, string name, IterationType type, Instant? startDate, Instant? endDate, Guid? teamId, AzdoIterationMetadata metadata, Instant timestamp)
    {
        Id = id;
        Name = name;
        Type = type;
        Start = startDate;
        End = endDate;
        TeamId = teamId;
        Metadata = metadata;

        SetState(timestamp);
    }

    public int Id { get; init; }
    public string Name { get; init; }
    public IterationType Type { get; init; }
    public Instant? Start { get; init; }
    public Instant? End { get; init; }
    public IterationState State { get; private set; }
    public Guid? TeamId { get; init; }
    public AzdoIterationMetadata Metadata { get; init; }

    private void SetState(Instant now)
    {
        var state = IterationState.Unknown;
        if (Start.HasValue && End.HasValue)
        {
            if (Start.Value > now)
            {
                state = IterationState.Future;
            }
            else if (End.Value < now)
            {
                state = IterationState.Completed;
            }
            else
            {
                state = IterationState.Active;
            }
        }
        else if (Start.HasValue && !End.HasValue)
        {
            state = Start.Value > now
                ? IterationState.Future
                : IterationState.Active;
        }
        else if (!Start.HasValue && End.HasValue)
        {
            state = End.Value < now
                ? IterationState.Completed
                : IterationState.Active;
        }

        State = state;
    }
}
