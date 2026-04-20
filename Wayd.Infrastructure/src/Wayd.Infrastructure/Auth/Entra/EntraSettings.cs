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
    /// Multi-tenant authority URL used for OIDC discovery and signature validation.
    /// Use <c>https://login.microsoftonline.com/common/v2.0</c> for multi-tenant —
    /// any tenant can issue a token, and we enforce which tenants we accept via
    /// <see cref="AllowedTenantIds"/>.
    /// </summary>
    public string Authority { get; set; } = "https://login.microsoftonline.com/common/v2.0";

    /// <summary>
    /// Expected <c>aud</c> claim on incoming id tokens. Pins tokens to this app
    /// registration — tokens issued for a different Microsoft app are rejected.
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
