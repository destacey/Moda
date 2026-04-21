namespace Wayd.Common.Application.Identity.Tokens;

/// <summary>
/// Advertises which authentication providers are enabled on this deployment.
/// Returned by <c>GET /api/auth/providers</c> so the frontend can render the
/// right set of login buttons without knowing the deployment's config.
/// Additive — new providers slot in as additional properties when onboarded.
/// </summary>
public sealed record AuthProvidersResponse(bool Local, bool Entra);
