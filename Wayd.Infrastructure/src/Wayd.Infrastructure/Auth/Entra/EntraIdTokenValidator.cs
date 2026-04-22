using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Wayd.Infrastructure.Auth.Entra;

internal sealed class EntraIdTokenValidator : IEntraIdTokenValidator
{
    private readonly IConfiguration _config;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _oidcConfigManager;
    private readonly ILogger<EntraIdTokenValidator> _logger;

    public EntraIdTokenValidator(
        IConfiguration config,
        IConfigurationManager<OpenIdConnectConfiguration> oidcConfigManager,
        ILogger<EntraIdTokenValidator> logger)
    {
        _config = config;
        _oidcConfigManager = oidcConfigManager;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> Validate(string subjectToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subjectToken))
        {
            throw new UnauthorizedException("Invalid token.");
        }

        var settings = GetSettings();

        if (settings.AllowedTenantIds.Count == 0)
        {
            // Defense-in-depth. Startup validation in AddEntraTokenExchange already
            // rejects an empty allowlist, so this branch only fires if config is
            // mutated at runtime (e.g., reload from a configuration provider) to
            // an invalid state. Warning, not Error — the startup guard is the
            // primary defense and surfaces the real misconfiguration.
            _logger.LogWarning("SecuritySettings:Providers:Entra:AllowedTenantIds is empty at runtime; rejecting exchange.");
            throw new UnauthorizedException("Invalid token.");
        }

        OpenIdConnectConfiguration oidcConfig;
        try
        {
            oidcConfig = await _oidcConfigManager.GetConfigurationAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch OIDC discovery document from {Authority}.", settings.Authority);
            throw new UnauthorizedException("Invalid token.");
        }

        var validationParameters = new TokenValidationParameters
        {
            // Signature validation happens against Microsoft's JWKS. Issuer varies
            // per tenant under /common/, so we skip issuer validation here and do
            // it ourselves via the tenant allowlist below — the tid claim is what
            // we actually trust.
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = oidcConfig.SigningKeys,

            ValidateIssuer = false,

            ValidateAudience = true,
            ValidAudience = settings.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),

            // Pin the algorithm. Entra uses RS256; accepting "none" or HS256 here
            // would be a classic alg-confusion hole even with an IssuerSigningKey
            // set, because some libraries will happily use the public key as an
            // HMAC secret if asked.
            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
        };

        ClaimsPrincipal principal;
        try
        {
            // MapInboundClaims = false keeps JWT short names (tid, iss, sub) on
            // the principal. The default map rewrites them to long schema URLs
            // (e.g. http://schemas.microsoft.com/identity/claims/tenantid), which
            // would break the tid/iss lookups below. Short names are the OIDC
            // standard; preserving them is what every modern OIDC library does.
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            principal = handler.ValidateToken(subjectToken, validationParameters, out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            // SecurityTokenException covers signature/audience/lifetime/algorithm
            // failures. ArgumentException covers malformed-token cases (non-JWT
            // input, bad base64, etc.) — the handler throws these before it even
            // reaches the token-validation pipeline.
            _logger.LogWarning(ex, "Entra id token validation failed.");
            throw new UnauthorizedException("Invalid token.");
        }

        // Tenant allowlist — this is the multi-tenant guard. Without an entry
        // here, an otherwise-valid Entra token from any other org is rejected.
        // Case-insensitive compare because operators occasionally enter tenant
        // IDs with mixed case; Entra itself always issues them lowercase.
        //
        // We prefer the `tid` claim (standard on org-context tokens) but fall
        // back to extracting the tenant from `iss` when tid is absent. Personal
        // Microsoft accounts acting as guests, and some v2.0 edge cases, omit
        // tid. The issuer is signed along with everything else, so deriving the
        // tenant from it is just as trustworthy as reading tid directly.
        var tid =
            principal.FindFirstValue("tid")
            ?? ExtractTenantFromIssuer(principal.FindFirstValue("iss"));

        if (string.IsNullOrWhiteSpace(tid) ||
            !settings.AllowedTenantIds.Contains(tid, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Entra id token rejected: tenant {Tid} not in allowlist.",
                tid ?? "<missing>");
            throw new UnauthorizedException("Invalid token.");
        }

        return principal;
    }

    /// <summary>
    /// Parses the tenant GUID from an Entra issuer URL. Handles both v1 and v2
    /// shapes:
    ///   v1: https://sts.windows.net/{tid}/
    ///   v2: https://login.microsoftonline.com/{tid}/v2.0
    /// Returns the first path segment if it parses as a GUID, otherwise null.
    /// <para>
    /// This is a shape check, not a trust decision — the MSA (personal accounts)
    /// pseudo-tenant <c>9188040d-6c67-4c5b-b112-36a304b66dad</c> is a real GUID
    /// and <em>is</em> returned. Keeping personal accounts out is the
    /// allowlist's job, not this function's. Non-GUID path segments like
    /// <c>common</c> and <c>organizations</c> return null because they aren't
    /// tenant IDs in the first place.
    /// </para>
    /// </summary>
    internal static string? ExtractTenantFromIssuer(string? issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer)) return null;
        if (!Uri.TryCreate(issuer, UriKind.Absolute, out var uri)) return null;

        // First path segment is the tenant ID. Filter to GUIDs so non-tenant
        // authority markers (common, organizations, consumers) don't leak
        // through as "tenants" the allowlist then has to special-case.
        var segments = uri.Segments
            .Select(s => s.Trim('/'))
            .Where(s => s.Length > 0)
            .ToArray();
        if (segments.Length == 0) return null;

        var candidate = segments[0];
        return Guid.TryParse(candidate, out _) ? candidate : null;
    }

    private EntraSettings GetSettings()
    {
        var settings = _config.GetSection(EntraSettings.SectionName).Get<EntraSettings>();
        if (settings is null || string.IsNullOrWhiteSpace(settings.Authority) || string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException($"Configuration section '{EntraSettings.SectionName}' is missing required values.");
        }

        return settings;
    }
}
