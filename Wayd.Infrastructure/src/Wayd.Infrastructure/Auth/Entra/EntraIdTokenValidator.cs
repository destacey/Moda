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
            var handler = new JwtSecurityTokenHandler();
            principal = handler.ValidateToken(subjectToken, validationParameters, out _);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Entra id token validation failed.");
            throw new UnauthorizedException("Invalid token.");
        }

        // Tenant allowlist — this is the multi-tenant guard. Without an entry here,
        // an otherwise-valid Entra token from any other org is rejected. Case-
        // insensitive compare because operators occasionally enter tenant IDs with
        // mixed case (copy-paste from scripts, docs, etc.); Entra itself always
        // issues them lowercase, so normalizing to OrdinalIgnoreCase here just
        // makes misconfiguration debuggable rather than silently broken.
        var tid = principal.FindFirstValue("tid");
        if (string.IsNullOrWhiteSpace(tid) ||
            !settings.AllowedTenantIds.Contains(tid, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Entra id token rejected: tenant {Tid} not in allowlist.", tid ?? "<missing>");
            throw new UnauthorizedException("Invalid token.");
        }

        return principal;
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
