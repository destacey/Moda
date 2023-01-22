using Microsoft.AspNetCore.Authorization;
using Moda.Common.Domain.Authorization;

namespace Moda.Web.BlazorClient.Infrastructure.Auth;

public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string action, string resource) =>
        Policy = ApplicationPermission.NameFor(action, resource);
}
