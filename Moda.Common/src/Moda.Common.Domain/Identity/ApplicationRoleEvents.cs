using NodaTime;

namespace Moda.Common.Domain.Identity;

public abstract record ApplicationRoleEvent : DomainEvent
{
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    protected ApplicationRoleEvent(string roleId, string roleName, Instant triggeredOn) =>
        (RoleId, RoleName, TriggeredOn) = (roleId, roleName, triggeredOn);
}

public record ApplicationRoleCreatedEvent : ApplicationRoleEvent
{
    public ApplicationRoleCreatedEvent(string roleId, string roleName, Instant triggeredOn)
        : base(roleId, roleName, triggeredOn)
    {
    }
}

public record ApplicationRoleUpdatedEvent : ApplicationRoleEvent
{
    public bool PermissionsUpdated { get; set; }

    public ApplicationRoleUpdatedEvent(string roleId, string roleName, Instant triggeredOn, bool permissionsUpdated = false)
        : base(roleId, roleName, triggeredOn) =>
        PermissionsUpdated = permissionsUpdated;
}

public record ApplicationRoleDeletedEvent : ApplicationRoleEvent
{
    public bool PermissionsUpdated { get; set; }

    public ApplicationRoleDeletedEvent(string roleId, string roleName, Instant triggeredOn)
        : base(roleId, roleName, triggeredOn)
    {
    }
}