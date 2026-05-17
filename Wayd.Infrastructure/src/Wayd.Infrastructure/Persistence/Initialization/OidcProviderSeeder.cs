using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wayd.Common.Application.Identity;
using Wayd.Infrastructure.Auth.Entra;

namespace Wayd.Infrastructure.Persistence.Initialization;

/// <summary>
/// One-time bridge: if this deployment is configured for Entra via
/// <c>SecuritySettings:Providers:Entra</c> and there isn't yet an
/// <see cref="OidcProvider"/> row for Microsoft Entra ID, seed one.
/// Existing deployments roll forward into the database-managed provider
/// model without further operator action beyond adding
/// <c>SecuritySettings:Providers:Entra:SpaClientId</c>.
/// </summary>
/// <remarks>
/// The seeder only inserts. It deliberately does NOT update an existing row:
/// once the row exists the database is the source of truth. Sync-from-config
/// on every startup would silently undo admin edits made through the Settings UI.
/// </remarks>
public class OidcProviderSeeder : ICustomSeeder
{
    private readonly EntraSettings _entraSettings;
    private readonly ILogger<OidcProviderSeeder> _logger;

    public OidcProviderSeeder(
        IOptions<EntraSettings> entraSettings,
        ILogger<OidcProviderSeeder> logger)
    {
        _entraSettings = entraSettings.Value;
        _logger = logger;
    }

    public async Task Initialize(WaydDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
    {
        if (!_entraSettings.Enabled)
        {
            // Local-only deployment. Nothing to seed; the registry just won't have
            // any OIDC providers, and the login page will show only the local form.
            return;
        }

        var existing = await dbContext.OidcProviders
            .AnyAsync(p => p.Name == LoginProviders.MicrosoftEntraId, cancellationToken);
        if (existing)
        {
            return;
        }

        // SpaClientId is the SPA (frontend) app registration client ID — the value
        // the OIDC client sends as client_id when initiating the authorization flow.
        // Audience is the API app registration client ID — the expected aud claim.
        // Standard Entra two-registration setup: both values are required.
        if (string.IsNullOrWhiteSpace(_entraSettings.SpaClientId))
        {
            _logger.LogWarning(
                "Entra is enabled but SecuritySettings:Providers:Entra:SpaClientId is not set. " +
                "Skipping OidcProvider seed — add SpaClientId (the SPA/client app registration's " +
                "client ID) to SecuritySettings:Providers:Entra, then restart to seed automatically, " +
                "or create the provider manually via the Settings UI.");
            return;
        }

        var displayName = "Microsoft Entra ID";
        var baseScopes = new[] { "openid", "profile", "email" };
        var scopes = string.IsNullOrWhiteSpace(_entraSettings.ApiScope)
            ? baseScopes
            : [.. baseScopes, _entraSettings.ApiScope];

        var result = OidcProvider.Create(
            name: LoginProviders.MicrosoftEntraId,
            displayName: displayName,
            providerType: OidcProviderType.MicrosoftEntraId,
            authority: _entraSettings.Authority,
            clientId: _entraSettings.SpaClientId!,
            audience: _entraSettings.Audience,
            scopes: scopes,
            allowedTenantIds: _entraSettings.AllowedTenantIds,
            clockSkewSeconds: _entraSettings.ClockSkewSeconds,
            isEnabled: true,
            timestamp: dateTimeProvider.Now);

        if (result.IsFailure)
        {
            // The existing Entra config doesn't satisfy the new entity's invariants
            // (e.g. AllowedTenantIds empty). Log and skip rather than crash — the
            // legacy Entra token-exchange path keeps working until the admin fixes
            // config or creates the row through the Settings UI.
            _logger.LogWarning(
                "Could not seed Microsoft Entra ID OidcProvider from existing config: {Error}",
                result.Error);
            return;
        }

        dbContext.OidcProviders.Add(result.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded Microsoft Entra ID OidcProvider from existing SecuritySettings config. " +
            "Manage providers via the Settings UI from now on.");
    }
}
