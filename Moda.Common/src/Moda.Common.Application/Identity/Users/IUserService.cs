using System.Security.Claims;

namespace Moda.Common.Application.Identity.Users;

public interface IUserService : ITransientService
{
    Task<IReadOnlyList<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken);

    Task<bool> ExistsWithNameAsync(string name);
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);

    Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken);

    Task<int> GetCountAsync(CancellationToken cancellationToken);

    Task<UserDetailsDto?> GetAsync(string userId, CancellationToken cancellationToken);

    Task<string?> GetEmailAsync(string userId);

    Task<List<UserRoleDto>> GetRolesAsync(string userId, bool includeUnassigned, CancellationToken cancellationToken);
    Task<Result> AssignRolesAsync(AssignUserRolesCommand command, CancellationToken cancellationToken);

    Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken);
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    Task ToggleStatusAsync(ToggleUserStatusCommand command, CancellationToken cancellationToken);

    Task<(string Id, string? EmployeeId)> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal);
    Task UpdateAsync(UpdateUserCommand command, string userId);
    Task<Result> UpdateMissingEmployeeIds(CancellationToken cancellationToken);

    Task<Result> SyncUsersFromEmployeeRecords(List<IExternalEmployee> externalEmployees, CancellationToken cancellationToken);

}