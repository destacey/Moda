using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Moda.Infrastructure.Auth.Permissions;

internal class AnyPermissionAuthorizationHandler(IUserService userService) : AuthorizationHandler<AnyPermissionRequirement>
{
    private readonly IUserService _userService = userService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyPermissionRequirement requirement)
    {
        if (context.User?.GetUserId() is not { } userId)
            return;

        // Check if user has ANY of the required permissions
        foreach (var permission in requirement.Permissions)
        {
            if (await _userService.HasPermissionAsync(userId, permission))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
