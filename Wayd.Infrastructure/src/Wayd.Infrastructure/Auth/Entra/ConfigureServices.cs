using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Wayd.Infrastructure.Auth.Entra;

internal static class ConfigureServices
{
    /// <summary>
    /// Registers services for the Entra token-exchange flow. Called from the
    /// top-level Auth pipeline; additive to the existing <c>AzureAd</c>
    /// middleware-based registration, which remains in place until the frontend
    /// migrates to the exchange flow.
    /// </summary>
    internal static IServiceCollection AddEntraTokenExchange(this IServiceCollection services, IConfiguration config)
    {
        // Bind settings even when disabled — the runtime 503 path reads Enabled
        // from IOptions, and the capabilities endpoint reads it to report whether
        // Entra is on for this deployment.
        services.Configure<EntraSettings>(config.GetSection(EntraSettings.SectionName));

        var settings = config.GetSection(EntraSettings.SectionName).Get<EntraSettings>()
            ?? new EntraSettings();

        if (!settings.Enabled)
        {
            // Local-only deployment. Skip validation + the JWKS ConfigurationManager
            // registration so the app boots without requiring Entra config. A stub
            // validator keeps the DI graph resolvable for TokenService, which gates
            // on Enabled and returns 503 before ever reaching the validator.
            services.AddScoped<IEntraIdTokenValidator, DisabledEntraIdTokenValidator>();
            return services;
        }

        if (string.IsNullOrWhiteSpace(settings.Authority))
        {
            throw new InvalidOperationException($"'{EntraSettings.SectionName}:Authority' must be configured when Entra is enabled.");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException($"'{EntraSettings.SectionName}:Audience' must be configured when Entra is enabled.");
        }

        if (settings.AllowedTenantIds is null || settings.AllowedTenantIds.Count == 0)
        {
            // An empty allowlist would reject every exchange attempt at runtime —
            // a misconfiguration, not a security feature. Failing fast at startup
            // surfaces the mistake before it turns into a wave of 401s in prod.
            throw new InvalidOperationException(
                $"'{EntraSettings.SectionName}:AllowedTenantIds' must contain at least one tenant ID when Entra is enabled.");
        }

        // Single process-wide ConfigurationManager handles OIDC discovery + JWKS
        // caching + automatic refresh. Designed to be long-lived; rebuilding
        // per-request defeats the caching.
        services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
            new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{settings.Authority.TrimEnd('/')}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true }));

        services.AddScoped<IEntraIdTokenValidator, EntraIdTokenValidator>();

        return services;
    }
}
