using System.Security.Claims;

namespace Wayd.Common.Application.Interfaces;

public interface ICurrentUser
{
    string? Name { get; }

    string GetUserId();

    Guid? GetEmployeeId();

    string? GetUserEmail();

    bool IsAuthenticated();

    bool IsInRole(string role);

    bool HasClaim(string type, string value);

    IEnumerable<Claim>? GetUserClaims();

    Task<bool> HasPermission(string permission, CancellationToken cancellationToken = default);
}
