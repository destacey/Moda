using System.Security.Claims;

namespace Wayd.Infrastructure.Auth.Entra;

/// <summary>
/// Validates an Entra id token for the token-exchange flow. Handles signature
/// verification via OIDC discovery (JWKS), audience + expiry checks, and
/// enforces the tenant allowlist. Returns the validated principal on success;
/// throws <see cref="UnauthorizedException"/> on any validation failure.
/// </summary>
internal interface IEntraIdTokenValidator : IScopedService
{
    Task<ClaimsPrincipal> Validate(string idToken, CancellationToken cancellationToken);
}
