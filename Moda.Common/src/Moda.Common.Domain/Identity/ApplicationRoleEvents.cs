using NodaTime;

namespace Moda.Common.Domain.Identity;

public abstract record ApplicationRoleEvent : DomainEvent
{
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    protected ApplicationRoleEvent(string roleId, string roleName, Instant timestamp) =>
        (RoleId, RoleName, Timestamp) = (roleId, roleName, timestamp);
}

public record ApplicationRoleCreatedEvent : ApplicationRoleEvent
{
    public ApplicationRoleCreatedEvent(string roleId, string roleName, Instant timestamp)
        : base(roleId, roleName, timestamp)
    {
    }
}

public record ApplicationRoleUpdatedEvent : ApplicationRoleEvent
{
    public bool PermissionsUpdated { get; set; }

    public ApplicationRoleUpdatedEvent(string roleId, string roleName, Instant timestamp, bool permissionsUpdated = false)
        : base(roleId, roleName, timestamp) =>
        PermissionsUpdated = permissionsUpdated;
}

public record ApplicationRoleDeletedEvent : ApplicationRoleEvent
{
    public bool PermissionsUpdated { get; set; }

    public ApplicationRoleDeletedEvent(string roleId, string roleName, Instant timestamp)
        : base(roleId, roleName, timestamp)
    {
    }
}