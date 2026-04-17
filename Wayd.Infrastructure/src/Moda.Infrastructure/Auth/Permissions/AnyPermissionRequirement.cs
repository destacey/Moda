using Microsoft.AspNetCore.Authorization;

namespace Wayd.Infrastructure.Auth.Permissions;

internal class AnyPermissionRequirement(IEnumerable<string> permissions) : IAuthorizationRequirement
{
    public IReadOnlyList<string> Permissions { get; } = permissions.ToList().AsReadOnly();
}
