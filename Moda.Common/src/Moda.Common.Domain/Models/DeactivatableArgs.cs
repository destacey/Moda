using NodaTime;

namespace Moda.Common.Domain.Models;
public abstract record DeactivatableArgs
{
    public Instant Timestamp { get; init; }
}
