namespace Moda.Web.Api.Models.UserManagement.Users;

public sealed record AssignUserRolesRequest
{
    public required string UserId { get; set; }
    public List<string> RoleNames { get; set; } = [];

    public AssignUserRolesCommand ToAssignUserRolesRequest()
        => new(UserId, RoleNames);
}
