using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Moda.Infrastructure.Auth;

public class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal? _user;
    private Guid _userId = Guid.Empty;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Lazily access user from HttpContext when available, otherwise use _user set via SetCurrentUser
    private ClaimsPrincipal? User => _user ?? _httpContextAccessor.HttpContext?.User;

    public string? Name => User?.Identity?.Name;

    public Guid GetUserId() =>
        IsAuthenticated()
            ? Guid.Parse(User?.GetUserId() ?? Guid.Empty.ToString())
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
        if (_userId != Guid.Empty)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        if (!string.IsNullOrEmpty(userId))
        {
            _userId = Guid.Parse(userId);
        }
    }
}