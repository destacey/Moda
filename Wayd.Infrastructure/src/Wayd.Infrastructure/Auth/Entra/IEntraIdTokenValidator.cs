using System.Security.Claims;

namespace Wayd.Infrastructure.Auth.Entra;

/// <summary>
/// Validates an Entra-issued token presented to the token-exchange endpoint.
/// For the Entra flow this is typically the access token MSAL acquires for the
/// API scope. Handles signature verification via OIDC discovery (JWKS),
/// audience + expiry checks, and enforces the tenant allowlist. Returns the
/// validated principal on success; throws <see cref="UnauthorizedException"/>
/// on any validation failure.
/// </summary>
internal interface IEntraIdTokenValidator
{
    Task<ClaimsPrincipal> Validate(string subjectToken, CancellationToken cancellationToken);
}
