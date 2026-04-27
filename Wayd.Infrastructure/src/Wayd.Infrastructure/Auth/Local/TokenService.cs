using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Wayd.Common.Application.Identity;
using Wayd.Common.Application.Identity.Tokens;
using Wayd.Common.Application.Identity.Users;
using Wayd.Common.Domain.Authorization;
using Wayd.Infrastructure.Auth.Entra;
using Wayd.Infrastructure.Identity;

namespace Wayd.Infrastructure.Auth.Local;

internal class TokenService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration config,
    IDateTimeProvider dateTimeProvider,
    IUserIdentityStore userIdentityStore,
    IUserService userService,
    IEntraIdTokenValidator entraIdTokenValidator,
    IOptions<EntraSettings> entraSettings,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IConfiguration _config = config;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IUserIdentityStore _userIdentityStore = userIdentityStore;
    private readonly IUserService _userService = userService;
    private readonly IEntraIdTokenValidator _entraIdTokenValidator = entraIdTokenValidator;
    private readonly EntraSettings _entraSettings = entraSettings.Value;
    private readonly ILogger<TokenService> _logger = logger;

    public async Task<TokenResponse> GetTokenAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(command.UserName);
        if (user is null)
        {
            _logger.LogWarning("Login failed: user {UserName} not found.", command.UserName);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (user.LoginProvider != LoginProviders.Wayd)
        {
            _logger.LogWarning("Login failed: user {UserName} is not a Wayd account (provider: {LoginProvider}).", command.UserName, user.LoginProvider);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("Login failed: user {UserName} is locked out.", command.UserName);
            throw new UnauthorizedException("Account is locked due to multiple failed login attempts. Please try again later.");
        }

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("Login failed: invalid password for user {UserName}.", command.UserName);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // Check inactive status only after credentials are validated
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: user {UserName} is inactive.", command.UserName);
            throw new UnauthorizedException("Your account has been deactivated. Please contact an administrator.");
        }

        await EnsureActiveIdentityAsync(user, LoginProviders.Wayd, command.UserName, cancellationToken);

        return await GenerateTokensAndUpdateUser(user, cancellationToken);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var userPrincipal = GetPrincipalFromExpiredToken(command.Token);
        var userId = userPrincipal.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("Invalid token.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid token.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive.");
        }

        if (user.RefreshToken != command.RefreshToken || user.RefreshTokenExpiryTime is not { } expiry || expiry <= _dateTimeProvider.Now.ToDateTimeUtc())
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        // Deactivating a UserIdentity must also stop in-flight sessions, not just new
        // logins. Without this check a user whose identity was revoked could keep
        // minting fresh access tokens via refresh until the refresh-token TTL
        // (days) elapsed. Provider is whatever the user is currently linked to —
        // an Entra-exchanged user requires an active Entra identity; a local user
        // requires an active Wayd identity.
        await EnsureActiveIdentityAsync(user, user.LoginProvider, user.UserName ?? userId, cancellationToken);

        return await GenerateTokensAndUpdateUser(user, cancellationToken);
    }

    public async Task<TokenResponse> ExchangeTokenAsync(ExchangeTokenCommand command, CancellationToken cancellationToken)
    {
        if (command.Provider != LoginProviders.MicrosoftEntraId)
        {
            // Auth0 and other providers are out of scope for PR 3.1 — the shape
            // generalizes cleanly when they're added, but we reject unknown
            // providers explicitly rather than silently falling through.
            _logger.LogWarning("Token exchange rejected: unsupported provider {Provider}.", command.Provider);
            throw new UnauthorizedException("Invalid token.");
        }

        if (!_entraSettings.Enabled)
        {
            // Local-only deployment — the feature exists but isn't configured here.
            // 503 distinguishes this from 401 (bad token) and 404 (no route): the
            // endpoint is discoverable, just turned off for this tenant.
            throw new ServiceUnavailableException("Entra token exchange is not enabled on this deployment.");
        }

        var principal = await _entraIdTokenValidator.Validate(command.SubjectToken, cancellationToken);

        // Reuse the existing principal-based user resolution. That path handles
        // UserIdentity lookup, the null-tid upgrade, new-user creation, and the
        // first-user-is-admin case — all unchanged from the MSAL middleware flow
        // this endpoint is replacing.
        var (userId, _) = await _userService.GetOrCreateFromPrincipalAsync(principal);

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedException("Invalid token.");

        if (!user.IsActive)
        {
            _logger.LogWarning("Exchange failed: user {UserId} is inactive.", user.Id);
            throw new UnauthorizedException("Your account has been deactivated. Please contact an administrator.");
        }

        await EnsureActiveIdentityAsync(user, LoginProviders.MicrosoftEntraId, user.UserName ?? user.Id, cancellationToken);

        return await GenerateTokensAndUpdateUser(user, cancellationToken);
    }

    private async Task EnsureActiveIdentityAsync(ApplicationUser user, string provider, string usernameForLogging, CancellationToken cancellationToken)
    {
        // Requires an active UserIdentity row for the given provider. Enables
        // "disable login for this user" by deactivating the identity row — no new
        // flag needed. Applied on login, refresh, and exchange so revocation takes
        // effect on the user's next refresh, not when the refresh token expires.
        var hasActiveIdentity = await _userIdentityStore.ExistsActive(user.Id, provider, cancellationToken);
        if (!hasActiveIdentity)
        {
            _logger.LogWarning("Authentication failed: user {UserName} has no active {Provider} identity.", usernameForLogging, provider);
            throw new UnauthorizedException("Invalid credentials.");
        }
    }

    private async Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, CancellationToken cancellationToken)
    {
        var settings = GetSettings();

        // Permissions are embedded in the JWT as claims so the frontend doesn't
        // need a separate /permissions fetch on load. Re-read on every issuance
        // (including refresh), so an admin permission change takes effect on the
        // user's next refresh — no version tracking needed, TTL is the revocation
        // clock.
        var permissions = await _userService.GetPermissionsAsync(user.Id, cancellationToken);

        var token = GenerateJwt(user, permissions, settings);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = _dateTimeProvider.Now.ToDateTimeUtc().AddDays(settings.RefreshTokenExpirationInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        var tokenExpiry = _dateTimeProvider.Now.ToDateTimeUtc().AddMinutes(settings.TokenExpirationInMinutes);

        return new TokenResponse(token, refreshToken, tokenExpiry, user.MustChangePassword);
    }

    private string GenerateJwt(ApplicationUser user, IReadOnlyList<string> permissions, LocalJwtSettings settings)
    {
        var key = ConfigureServices.CreateSigningKey(settings.Secret);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            // Frontend reads this to drive provider-specific UX (e.g. showing the
            // "Change Password" button only for local users) and to gate the
            // forced-password-change flow. Without it, authMethod is null for
            // every session and those branches silently disable themselves.
            new("loginProvider", user.LoginProvider),
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
        }

        // One claim per permission (ASP.NET Core idiom). Enables both
        // ClaimsPrincipal.HasClaim("permission", ...) on the server and a
        // uniform token shape across all login providers.
        foreach (var permission in permissions)
        {
            claims.Add(new Claim(ApplicationClaims.Permission, permission));
        }

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: _dateTimeProvider.Now.ToDateTimeUtc().AddMinutes(settings.TokenExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var settings = GetSettings();

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = ConfigureServices.CreateSigningKey(settings.Secret),
            ValidateLifetime = false, // Allow expired tokens for refresh
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new UnauthorizedException("Invalid token.");
        }

        return principal;
    }

    private LocalJwtSettings GetSettings()
    {
        var settings = _config.GetSection(LocalJwtSettings.SectionName).Get<LocalJwtSettings>();
        if (settings is null || string.IsNullOrWhiteSpace(settings.Secret))
        {
            throw new InvalidOperationException("Local JWT settings are not configured.");
        }

        return settings;
    }

    public AuthProvidersResponse GetAuthProviders() =>
        new(Local: true, Entra: _entraSettings.Enabled);
}
