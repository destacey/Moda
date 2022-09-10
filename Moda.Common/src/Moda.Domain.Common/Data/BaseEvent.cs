using MediatR;
using NodaTime;

namespace Moda.Common.Domain.Data;

public abstract class BaseEvent : INotification
{
    public Guid Id { get; set; }
    public Instant Created { get; set; }
}
