using System.Security.Claims;

namespace Moda.Common.Application.Interfaces;

public interface ICurrentUser
{
    string? Name { get; }

    Guid GetUserId();

    string? GetUserEmail();

    bool IsAuthenticated();

    bool IsInRole(string role);

    bool HasClaim(string type, string value);

    IEnumerable<Claim>? GetUserClaims();
}
