using NodaTime;

namespace Wayd.Common.Domain.Models;

public abstract record ActivatableArgs
{
    public Instant Timestamp { get; protected init; }
}
