using System.Security.Claims;

namespace Moda.Core.Application.Identity.Users;

public interface IUserService : ITransientService
{
    Task<PaginationResponse<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken);

    Task<bool> ExistsWithNameAsync(string name);
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);

    Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken);

    Task<int> GetCountAsync(CancellationToken cancellationToken);

    Task<UserDetailsDto> GetAsync(string userId, CancellationToken cancellationToken);

    Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken);
    Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken);

    Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken);
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken);

    Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal);
    Task UpdateAsync(UpdateUserRequest request, string userId);
}