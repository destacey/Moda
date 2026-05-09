namespace Wayd.Web.Api.Models.Organizations.TeamMemberRoles;

public sealed record CreateTeamMemberRoleRequest(string Name);

public sealed record UpdateTeamMemberRoleRequest(Guid Id, string Name);
