using Microsoft.AspNetCore.Authorization;
using Moda.Common.Domain.Authorization;
using System.Security.Claims;

namespace Moda.Web.BlazorClient.Infrastructure.Auth;

public static class AuthorizationServiceExtensions
{
    public static async Task<bool> HasPermissionAsync(this IAuthorizationService service, ClaimsPrincipal user, string action, string resource) =>
        (await service.AuthorizeAsync(user, null, ApplicationPermission.NameFor(action, resource))).Succeeded;
}
