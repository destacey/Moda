using NodaTime;

namespace Moda.Common.Domain.Identity;

public abstract record ApplicationUserEvent : DomainEvent
{
    public string UserId { get; set; } = default!;

    protected ApplicationUserEvent(string userId, Instant triggeredOn) => 
        (UserId, TriggeredOn) = (userId, triggeredOn);
}

public record ApplicationUserCreatedEvent : ApplicationUserEvent
{
    public ApplicationUserCreatedEvent(string userId, Instant triggeredOn)
        : base(userId, triggeredOn)
    {
    }
}

public record ApplicationUserUpdatedEvent : ApplicationUserEvent
{
    public bool RolesUpdated { get; set; }

    public ApplicationUserUpdatedEvent(string userId, Instant triggeredOn, bool rolesUpdated = false)
        : base(userId, triggeredOn) =>
        RolesUpdated = rolesUpdated;
}