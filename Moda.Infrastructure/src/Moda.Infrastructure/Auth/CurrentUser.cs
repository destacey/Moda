using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Wayd.Infrastructure.Auth;

public class CurrentUser(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider) : ICurrentUser, ICurrentUserInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private ClaimsPrincipal? _user;
    private string _userId = string.Empty;

    private HashSet<string>? _permissionsCache;

    // Lazily access user from HttpContext when available, otherwise use _user set via SetCurrentUser
    private ClaimsPrincipal? User => _user ?? _httpContextAccessor.HttpContext?.User;

    public string? Name => User?.Identity?.Name;

    public string GetUserId() =>
        IsAuthenticated()
            ? User?.GetUserId() ?? string.Empty
            : _userId;

    public Guid? GetEmployeeId()
    {
        if (IsAuthenticated())
        {
            var employeeId = User?.GetEmployeeId();
            if (Guid.TryParse(employeeId, out var employeeGuid))
                return employeeGuid;
        }

        return null;
    }

    public string? GetUserEmail() =>
        IsAuthenticated()
            ? User!.GetEmail()
            : string.Empty;

    public bool IsAuthenticated() =>
        User?.Identity?.IsAuthenticated is true;

    public bool IsInRole(string role) =>
        User?.IsInRole(role) is true;

    public bool HasClaim(string type, string value) =>
        User?.HasClaim(type, value) is true;

    public IEnumerable<Claim>? GetUserClaims() =>
        User?.Claims;


    public async Task<bool> HasPermission(string permission, CancellationToken cancellationToken = default)
    {
        if (_permissionsCache is null)
        {
            var userService = _serviceProvider.GetRequiredService<IUserService>();
            var permissions = await userService.GetPermissionsAsync(GetUserId(), cancellationToken);
            _permissionsCache = [.. permissions];
        }

        return _permissionsCache.Contains(permission);
    }

    public void SetCurrentUser(ClaimsPrincipal user)
    {
        if (_user != null)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        _user = user;
    }

    public void SetCurrentUserId(string userId)
    {
        if (!string.IsNullOrEmpty(_userId))
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        if (!string.IsNullOrEmpty(userId))
        {
            _userId = userId;
        }
    }
}