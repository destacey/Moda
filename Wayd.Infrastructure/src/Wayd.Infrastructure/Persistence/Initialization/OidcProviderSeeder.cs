using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wayd.Common.Application.Identity;
using Wayd.Common.Domain.Identity;
using Wayd.Infrastructure.Auth.AzureAd;
using Wayd.Infrastructure.Auth.Entra;

namespace Wayd.Infrastructure.Persistence.Initialization;

/// <summary>
/// One-time bridge: if this deployment is configured for Entra via the legacy
/// env/config settings and there isn't yet an <see cref="OidcProvider"/> row for
/// Microsoft Entra ID, seed one. Existing deployments roll forward into the new
/// database-managed provider model without operator action.
/// </summary>
/// <remarks>
/// The seeder only inserts. It deliberately does NOT update an existing row from
/// config: once the row exists, the database is the source of truth. Sync-from-
/// config-every-startup would silently undo admin edits made through the
/// Settings UI and create a confusing config/DB race.
///
/// The <c>ClientId</c> is sourced from <c>SecuritySettings:AzureAd:ClientId</c>,
/// which is currently used by the Graph integration and almost always matches
/// the frontend MSAL client. If it's not set, the seeder logs a warning and
/// skips — the admin can create the provider explicitly via the Settings UI
/// once Phase 2 ships. We don't fabricate a placeholder ClientId because that
/// would create a provider row that doesn't actually work.
/// </remarks>
public class OidcProviderSeeder : ICustomSeeder
{
    private readonly EntraSettings _entraSettings;
    private readonly AzureAdSettings _azureAdSettings;
    private readonly ILogger<OidcProviderSeeder> _logger;

    public OidcProviderSeeder(
        IOptions<EntraSettings> entraSettings,
        IOptions<AzureAdSettings> azureAdSettings,
        ILogger<OidcProviderSeeder> logger)
    {
        _entraSettings = entraSettings.Value;
        _azureAdSettings = azureAdSettings.Value;
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

        if (string.IsNullOrWhiteSpace(_azureAdSettings.ClientId))
        {
            _logger.LogWarning(
                "Entra is enabled but SecuritySettings:AzureAd:ClientId is empty. " +
                "Skipping OidcProvider seed — an admin can create the Microsoft Entra ID provider " +
                "through the Settings UI once configured. Frontend Entra login will continue using " +
                "the existing NEXT_PUBLIC_AZURE_AD_CLIENT_ID env var until then.");
            return;
        }

        var displayName = "Microsoft Entra ID";
        // Default scopes for the SPA flow. The frontend OIDC client uses these to
        // request the access token it then exchanges at /api/auth/exchange. The
        // backend only validates the resulting token's `aud` claim — it doesn't
        // enforce scopes — so getting these slightly wrong here is recoverable.
        // The admin can refine the list via the Settings UI.
        var scopes = new[] { "openid", "profile", "email" };

        var result = OidcProvider.Create(
            name: LoginProviders.MicrosoftEntraId,
            displayName: displayName,
            providerType: OidcProviderType.MicrosoftEntraId,
            authority: _entraSettings.Authority,
            clientId: _azureAdSettings.ClientId!,
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
