using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wayd.Common.Application.Identity.OidcProviders;
using Wayd.Infrastructure.Auth.Entra;

namespace Wayd.Infrastructure.Auth.Oidc;

internal static class ConfigureServices
{
    /// <summary>
    /// Registers the database-backed OIDC provider registry, the per-authority
    /// JWKS <c>ConfigurationManager</c> cache, the token validator, and binds
    /// <c>EntraSettings</c> — the latter so <see cref="OidcProviderSeeder"/> can
    /// translate the legacy <c>SecuritySettings:Providers:Entra</c> config into
    /// an <c>OidcProvider</c> row on first boot.
    /// </summary>
    internal static IServiceCollection AddOidcProviderRegistry(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOidcProviderRegistry, OidcProviderRegistry>();
        services.AddSingleton<IOidcConfigurationManagerFactory, OidcConfigurationManagerFactory>();

        // Scoped to match the rest of the request-bound auth surface. Its only
        // meaningful state is the logger, but lifetime parity matters because the
        // validator depends on the singleton registry/factory and not the other
        // way around — keeping it scoped avoids accidental upgrade to singleton.
        services.AddScoped<IOidcTokenValidator, OidcTokenValidator>();

        // Legacy Entra config schema. Read at startup by the OidcProvider seeder
        // to bootstrap a Microsoft Entra ID row for deployments that already had
        // SecuritySettings:Providers:Entra configured before the DB-managed
        // provider model existed. Removable in a future cleanup once all
        // deployments have rolled forward.
        services.Configure<EntraSettings>(config.GetSection(EntraSettings.SectionName));

        return services;
    }
}
