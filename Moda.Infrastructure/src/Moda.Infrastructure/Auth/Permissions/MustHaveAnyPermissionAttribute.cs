using Microsoft.AspNetCore.Authorization;

namespace Moda.Infrastructure.Auth.Permissions;

public class MustHaveAnyPermissionAttribute : AuthorizeAttribute
{
    public MustHaveAnyPermissionAttribute(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("At least one permission must be specified.", nameof(permissions));

        Policy = $"MustHaveAnyPermission:{string.Join(",", permissions)}";
    }

    public static MustHaveAnyPermissionAttribute ForActionResource(params (string action, string resource)[] actionResources)
    {
        var permissions = actionResources
            .Select(ar => ApplicationPermission.NameFor(ar.action, ar.resource))
            .ToArray();
        return new MustHaveAnyPermissionAttribute(permissions);
    }
}
