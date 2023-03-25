namespace Moda.Web.Api.Models.Identity.Users;

public sealed record AssignUserRolesRequest
{
    public string UserId { get; set; } = null!;
    public List<UserRoleDto> UserRoles { get; set; } = null!;

    public AssignUserRolesCommand ToAssignUserRolesRequest()
        => new(UserId, UserRoles);
}
