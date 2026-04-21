using System.Security.Claims;

namespace Wayd.Infrastructure.Auth.Entra;

/// <summary>
/// Stand-in used when <see cref="EntraSettings.Enabled"/> is false. Exists so
/// that <c>ITokenService</c> can still be constructed on local-only deployments.
/// Not reachable in practice — <c>TokenService.ExchangeTokenAsync</c> short-
/// circuits with a 503 before ever calling the validator. If that invariant is
/// ever broken, this stub fails loud instead of silently validating nothing.
/// </summary>
internal sealed class DisabledEntraIdTokenValidator : IEntraIdTokenValidator
{
    public Task<ClaimsPrincipal> Validate(string subjectToken, CancellationToken cancellationToken) =>
        throw new InvalidOperationException(
            "Entra token exchange is not enabled on this deployment. " +
            "This path should be unreachable — TokenService.ExchangeTokenAsync should have returned 503 before reaching the validator.");
}
