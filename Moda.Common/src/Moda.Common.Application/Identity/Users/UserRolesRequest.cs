namespace Moda.Common.Application.Identity.Users;

public sealed record UserRolesRequest
{
    public List<UserRoleDto> UserRoles { get; set; } = new();
}