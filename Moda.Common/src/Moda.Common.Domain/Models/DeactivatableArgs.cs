using NodaTime;

namespace Wayd.Common.Domain.Models;

public abstract record DeactivatableArgs
{
    public Instant Timestamp { get; protected init; }
}
