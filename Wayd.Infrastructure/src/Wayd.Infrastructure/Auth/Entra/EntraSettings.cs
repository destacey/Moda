namespace Wayd.Infrastructure.Auth.Entra;

/// <summary>
/// Config for the Microsoft Entra ID token-exchange flow. The frontend hands us
/// an Entra id token; we validate it, resolve the user, and mint a Wayd JWT.
///
/// Bound to the <c>SecuritySettings:Providers:Entra</c> section. The nested
/// <c>Providers</c> grouping anticipates Auth0 and any other OIDC providers
/// sitting at the same level (<c>SecuritySettings:Providers:Auth0</c>, etc.).
/// </summary>
public sealed class EntraSettings
{
    public const string SectionName = "SecuritySettings:Providers:Entra";

    /// <summary>
    /// Whether Entra token exchange is enabled on this deployment. Off by default
    /// so a local-only installation doesn't need to configure Authority/Audience/
    /// AllowedTenantIds. When <c>false</c>, the <c>/api/auth/exchange</c> endpoint
    /// responds with HTTP 503 and <see cref="EntraSettings"/> validation is skipped
    /// at startup.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Multi-tenant authority URL used for OIDC discovery and signature validation.
    /// Use <c>https://login.microsoftonline.com/common/v2.0</c> for multi-tenant —
    /// any tenant can issue a token, and we enforce which tenants we accept via
    /// <see cref="AllowedTenantIds"/>.
    /// </summary>
    public string Authority { get; set; } = "https://login.microsoftonline.com/common/v2.0";

    /// <summary>
    /// Client ID of the SPA (frontend) app registration. Used by the OIDC client
    /// when initiating the authorization flow. Distinct from <see cref="Audience"/>
    /// because Entra's standard two-registration pattern has a separate app
    /// registration for the SPA and the API.
    /// </summary>
    public string? SpaClientId { get; set; }

    /// <summary>
    /// The API scope to request alongside the standard OIDC scopes. This causes
    /// Entra to issue an access token with <c>aud</c> set to the API app
    /// registration's client ID, which is what <see cref="Audience"/> pins.
    /// Typically <c>api://{api-client-id}/access_as_user</c>.
    /// If omitted, only <c>openid profile email</c> are requested and the
    /// resulting token will have the SPA client ID as audience — validation fails.
    /// </summary>
    public string? ApiScope { get; set; }

    /// <summary>
    /// Expected <c>aud</c> claim on incoming id tokens. This is the API app
    /// registration's client ID — tokens issued for a different app are rejected.
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Tenants (<c>tid</c> values) that are allowed to authenticate through this
    /// app. Empty list rejects everything (fail-safe default). This is the
    /// multi-tenant gatekeeper — without an entry here, an otherwise-valid Entra
    /// token from any other org is turned away at the door.
    /// </summary>
    public IReadOnlyList<string> AllowedTenantIds { get; set; } = [];

    /// <summary>
    /// Clock skew tolerance for expiry/not-before checks on incoming id tokens.
    /// One minute is standard practice and matches the local JWT scheme.
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 60;
}
