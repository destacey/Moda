using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.Planning;
using NodaTime;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoIteration : IExternalIteration<AzdoIterationMetadata>
{
    public AzdoIteration(string id, string name, IterationType type, Instant? startDate, Instant? endDate, Guid? teamId, AzdoIterationMetadata metadata, Instant timestamp)
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

    public string Id { get; init; }
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
                state = IterationState.Closed;
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
                ? IterationState.Closed 
                : IterationState.Active;
        }

        State = state;
    }
}
