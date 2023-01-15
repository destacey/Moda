using NodaTime;

namespace Moda.Common.Domain.Models;
public abstract record ActivatableArgs
{
    public Instant Timestamp { get; init; }
}
