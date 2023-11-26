namespace Moda.Web.Api.Models.Identity.Users;

public sealed record AssignUserRolesRequest
{
    public required string UserId { get; set; }
    public List<UserRoleDto> UserRoles { get; set; } = new();

    public AssignUserRolesCommand ToAssignUserRolesRequest()
        => new(UserId, UserRoles);
}
