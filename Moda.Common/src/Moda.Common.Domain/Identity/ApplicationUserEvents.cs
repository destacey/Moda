using NodaTime;

namespace Moda.Common.Domain.Identity;

public abstract record ApplicationUserEvent : DomainEvent
{
    public string UserId { get; set; } = default!;

    protected ApplicationUserEvent(string userId, Instant timestamp) =>
        (UserId, Timestamp) = (userId, timestamp);
}

public record ApplicationUserCreatedEvent : ApplicationUserEvent
{
    public ApplicationUserCreatedEvent(string userId, Instant timestamp)
        : base(userId, timestamp)
    {
    }
}

public record ApplicationUserUpdatedEvent : ApplicationUserEvent
{
    public bool RolesUpdated { get; set; }

    public ApplicationUserUpdatedEvent(string userId, Instant timestamp, bool rolesUpdated = false)
        : base(userId, timestamp) =>
        RolesUpdated = rolesUpdated;
}