namespace Moda.Common.Application.Identity.Users;

public sealed record AssignUserRolesCommand
{
    public AssignUserRolesCommand(string userId, List<UserRoleDto> userRoles)
    {
        UserId = userId;
        UserRoles = userRoles ?? new List<UserRoleDto>();
    }

    public string UserId { get; }
    public List<UserRoleDto> UserRoles { get; }
}