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
        var settings = config.GetSection(EntraSettings.SectionName).Get<EntraSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{EntraSettings.SectionName}' is missing.");

        if (string.IsNullOrWhiteSpace(settings.Authority))
        {
            throw new InvalidOperationException($"'{EntraSettings.SectionName}:Authority' must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException($"'{EntraSettings.SectionName}:Audience' must be configured.");
        }

        // Single process-wide ConfigurationManager handles OIDC discovery + JWKS
        // caching + automatic refresh. Designed to be long-lived; rebuilding
        // per-request defeats the caching.
        services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
            new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{settings.Authority.TrimEnd('/')}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true }));

        return services;
    }
}
