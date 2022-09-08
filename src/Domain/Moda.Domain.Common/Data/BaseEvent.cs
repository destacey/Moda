using MediatR;
using NodaTime;

namespace Moda.Domain.Common.Data;

public abstract class BaseEvent : INotification
{
    public Guid Id { get; set; }
    public Instant Created { get; set; }
}
