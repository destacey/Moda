namespace Moda.Core.Application.Identity.Users;

public sealed class UserRolesRequest
{
    public List<UserRoleDto> UserRoles { get; set; } = new();
}