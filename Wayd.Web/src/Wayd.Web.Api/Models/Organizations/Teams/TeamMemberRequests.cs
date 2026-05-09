namespace Wayd.Web.Api.Models.Organizations.Teams;

public sealed record AddTeamMemberRequest(Guid EmployeeId, Guid RoleId);

public sealed record UpdateTeamMemberRequest(Guid RoleId);
