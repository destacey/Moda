using MediatR;
using NodaTime;

namespace Moda.Common.Domain.Events;

public abstract record DomainEvent : INotification, IHasCreationDateTime
{
    public Instant Created { get; }
}
