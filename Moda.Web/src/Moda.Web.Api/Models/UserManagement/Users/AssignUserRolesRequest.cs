namespace Moda.Web.Api.Models.UserManagement.Users;

public sealed record AssignUserRolesRequest
{
    public string UserId { get; set; } = default!;
    public List<string> RoleNames { get; set; } = [];

    public AssignUserRolesCommand ToAssignUserRolesRequest()
        => new(UserId, RoleNames);
}
