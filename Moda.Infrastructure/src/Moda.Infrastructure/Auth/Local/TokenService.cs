using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moda.Common.Application.Identity;
using Moda.Common.Application.Identity.Tokens;

namespace Moda.Infrastructure.Auth.Local;

internal class TokenService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration config,
    IDateTimeProvider dateTimeProvider,
    ILogger<TokenService> logger) : ITokenService
{
    public async Task<TokenResponse> GetTokenAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(command.UserName);
        if (user is null)
        {
            logger.LogWarning("Login failed: user {UserName} not found.", command.UserName);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (user.LoginProvider != LoginProviders.Moda)
        {
            logger.LogWarning("Login failed: user {UserName} is not a Moda account (provider: {LoginProvider}).", command.UserName, user.LoginProvider);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (signInResult.IsLockedOut)
        {
            logger.LogWarning("Login failed: user {UserName} is locked out.", command.UserName);
            throw new UnauthorizedException("Account is locked due to multiple failed login attempts. Please try again later.");
        }

        if (!signInResult.Succeeded)
        {
            logger.LogWarning("Login failed: invalid password for user {UserName}.", command.UserName);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // Check inactive status only after credentials are validated
        if (!user.IsActive)
        {
            logger.LogWarning("Login failed: user {UserName} is inactive.", command.UserName);
            throw new UnauthorizedException("Your account has been deactivated. Please contact an administrator.");
        }

        return await GenerateTokensAndUpdateUser(user);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var userPrincipal = GetPrincipalFromExpiredToken(command.Token);
        var userId = userPrincipal.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("Invalid token.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid token.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive.");
        }

        if (user.RefreshToken != command.RefreshToken || user.RefreshTokenExpiryTime <= dateTimeProvider.Now.ToDateTimeUtc())
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        return await GenerateTokensAndUpdateUser(user);
    }

    private async Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user)
    {
        var settings = GetSettings();
        var token = GenerateJwt(user, settings);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = dateTimeProvider.Now.ToDateTimeUtc().AddDays(settings.RefreshTokenExpirationInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiry;
        await userManager.UpdateAsync(user);

        var tokenExpiry = dateTimeProvider.Now.ToDateTimeUtc().AddMinutes(settings.TokenExpirationInMinutes);

        return new TokenResponse(token, refreshToken, tokenExpiry, user.MustChangePassword);
    }

    private string GenerateJwt(ApplicationUser user, LocalJwtSettings settings)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: dateTimeProvider.Now.ToDateTimeUtc().AddMinutes(settings.TokenExpirationInMinutes),
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
        var key = Encoding.UTF8.GetBytes(settings.Secret);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
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
        var settings = config.GetSection(LocalJwtSettings.SectionName).Get<LocalJwtSettings>();
        if (settings is null || string.IsNullOrWhiteSpace(settings.Secret))
        {
            throw new InvalidOperationException("Local JWT settings are not configured.");
        }

        return settings;
    }
}
