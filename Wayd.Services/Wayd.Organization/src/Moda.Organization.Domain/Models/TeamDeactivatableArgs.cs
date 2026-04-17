using Wayd.Common.Domain.Models;
using NodaTime;

namespace Wayd.Organization.Domain.Models;

public sealed record TeamDeactivatableArgs : DeactivatableArgs
{
    public required LocalDate AsOfDate { get; init; }

    public static TeamDeactivatableArgs Create(LocalDate asOfDate, Instant timestamp)
        => new() { AsOfDate = asOfDate, Timestamp = timestamp };
}
