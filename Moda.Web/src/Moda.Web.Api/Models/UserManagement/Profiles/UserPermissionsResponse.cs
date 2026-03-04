namespace Moda.Web.Api.Models.UserManagement.Profiles;

public sealed record UserPermissionsResponse(List<string> Permissions, Guid? EmployeeId);
