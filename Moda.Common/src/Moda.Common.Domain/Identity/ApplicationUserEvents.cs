namespace Moda.Common.Domain.Identity;

public abstract record ApplicationUserEvent : DomainEvent
{
    public string UserId { get; set; } = default!;

    protected ApplicationUserEvent(string userId) => UserId = userId;
}

public record ApplicationUserCreatedEvent : ApplicationUserEvent
{
    public ApplicationUserCreatedEvent(string userId)
        : base(userId)
    {
    }
}

public record ApplicationUserUpdatedEvent : ApplicationUserEvent
{
    public bool RolesUpdated { get; set; }

    public ApplicationUserUpdatedEvent(string userId, bool rolesUpdated = false)
        : base(userId) =>
        RolesUpdated = rolesUpdated;
}