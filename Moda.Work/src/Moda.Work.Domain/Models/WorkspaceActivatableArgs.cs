using NodaTime;

namespace Moda.Work.Domain.Models;
public sealed record WorkspaceActivatableArgs : ActivatableArgs
{
    public required WorkProcess WorkProcess { get; init; }

    public static WorkspaceActivatableArgs Create(Instant timestamp, WorkProcess workProcess)
        => new() { Timestamp = timestamp, WorkProcess = workProcess };
}
