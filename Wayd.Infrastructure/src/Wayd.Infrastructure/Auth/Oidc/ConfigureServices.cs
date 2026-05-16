using Microsoft.Extensions.DependencyInjection;

namespace Wayd.Infrastructure.Auth.Oidc;

internal static class ConfigureServices
{
    /// <summary>
    /// Registers the database-backed OIDC provider registry and the per-authority
    /// <c>ConfigurationManager</c> cache. These run alongside the existing
    /// Entra-only token exchange path until <c>TokenService.ExchangeTokenAsync</c>
    /// is migrated to use the registry (Phase 1e).
    /// </summary>
    internal static IServiceCollection AddOidcProviderRegistry(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOidcProviderRegistry, OidcProviderRegistry>();
        services.AddSingleton<IOidcConfigurationManagerFactory, OidcConfigurationManagerFactory>();

        // Validator is scoped to match the existing IEntraIdTokenValidator
        // lifetime — its only meaningful state is the logger, but scoped keeps
        // it consistent with the rest of the request-bound auth surface.
        services.AddScoped<IOidcTokenValidator, OidcTokenValidator>();
        return services;
    }
}
