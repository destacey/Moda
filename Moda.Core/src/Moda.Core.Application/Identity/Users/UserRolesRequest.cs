namespace Moda.Core.Application.Identity.Users;

public sealed record UserRolesRequest
{
    public List<UserRoleDto> UserRoles { get; set; } = new();
}