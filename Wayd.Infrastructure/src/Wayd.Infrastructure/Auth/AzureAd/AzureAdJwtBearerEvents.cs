using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Wayd.Infrastructure.Auth.Local;
using Serilog;

namespace Wayd.Infrastructure.Auth.AzureAd;

internal class AzureAdJwtBearerEvents : JwtBearerEvents
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public AzureAdJwtBearerEvents(ILogger logger, IConfiguration config) =>
        (_logger, _config) = (logger, config);

    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        // Safety net: if a cross-scheme token somehow reaches validation,
        // suppress the issuer mismatch so the correct scheme can handle it.
        if (context.Exception is SecurityTokenInvalidIssuerException)
        {
            context.NoResult();
            return Task.CompletedTask;
        }

        _logger.AuthenticationFailed(context.Exception);
        return base.AuthenticationFailed(context);
    }

    public override Task MessageReceived(MessageReceivedContext context)
    {
        // SignalR sends the access token as a query parameter for WebSocket connections
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }

        // Skip this scheme early for tokens not intended for it.
        // Peeking at the issuer before validation prevents IdentityModel
        // from emitting IDX10205 diagnostic noise during cross-scheme attempts.
        var localJwtSettings = _config.GetSection(LocalJwtSettings.SectionName).Get<LocalJwtSettings>();
        var localIssuer = localJwtSettings?.Issuer ?? "Wayd";
        var token = context.Token;
        if (token is null)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
            {
                token = authHeader["Bearer ".Length..];
            }
        }

        if (token is not null)
        {
            try
            {
                var jwt = new JsonWebToken(token);
                if (string.Equals(jwt.Issuer, localIssuer, StringComparison.OrdinalIgnoreCase))
                {
                    context.NoResult();
                    return Task.CompletedTask;
                }
            }
            catch (ArgumentException)
            {
                // Malformed token — let normal validation surface the error
            }
        }

        _logger.TokenReceived();
        return base.MessageReceived(context);
    }

    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        if (context.HttpContext.Items.TryGetValue("RegistrationDenied", out var message))
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message });
            return;
        }

        await base.Challenge(context);
    }

    /// <summary>
    /// This method contains the logic that validates the user and normalizes claims.
    /// </summary>
    /// <param name="context">The validated token context.</param>
    /// <returns>A task.</returns>
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal;
        string? issuer = principal?.GetIssuer();
        string? objectId = principal?.GetObjectId();
        _logger.TokenValidationStarted(objectId, issuer);

        if (principal is null || issuer is null || objectId is null)
        {
            _logger.TokenValidationFailed(objectId, issuer);
            throw new UnauthorizedException("Authentication Failed.");
        }

        // The caller comes from an admin-consented, recorded issuer.
        var identity = principal.Identities.First();

        // Lookup local user or create one if none exist.
        (string Id, string? EmployeeId) userData;
        try
        {
            userData = await context.HttpContext.RequestServices.GetRequiredService<IUserService>()
                .GetOrCreateFromPrincipalAsync(principal);
        }
        catch (ForbiddenException ex)
        {
            _logger.RegistrationDenied(objectId, ex.Message);
            // Store the exception so OnChallenge can write the 403 response.
            // Writing the response here and calling Fail() can race with the
            // handler's default challenge, so we defer to OnChallenge instead.
            context.HttpContext.Items["RegistrationDenied"] = ex.Message;
            context.Fail(ex);
            return;
        }

        // We use the nameidentifier claim to store the user id.
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        identity.TryRemoveClaim(idClaim);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userData.Id));

        if (!string.IsNullOrWhiteSpace(userData.EmployeeId))
        {
            identity.AddClaim(new Claim("EmployeeId", userData.EmployeeId));
        }

        // And the email claim for the email.
        var emailClaim = principal.FindFirst(ClaimTypes.Email);
        if (emailClaim is null)
        {
            var upnClaim = principal.FindFirst(ClaimTypes.Upn);
            if (upnClaim is not null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, upnClaim.Value));
            }
            else
            {
                var email = await context.HttpContext.RequestServices.GetRequiredService<IUserService>()
                    .GetEmailAsync(userData.Id);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            }
        }

        _logger.TokenValidationSucceeded(objectId, issuer);
    }
}

internal static class AzureAdJwtBearerEventsLoggingExtensions
{
    public static void AuthenticationFailed(this ILogger logger, Exception e) =>
        logger.Error("Authentication failed Exception: {e}", e);

    public static void TokenReceived(this ILogger logger) =>
        logger.Debug("Received a bearer token");

    public static void TokenValidationStarted(this ILogger logger, string? userId, string? issuer) =>
        logger.Debug("Token Validation Started for User: {userId} Issuer: {issuer}", userId, issuer);

    public static void TokenValidationFailed(this ILogger logger, string? userId, string? issuer) =>
        logger.Warning("Tenant is not registered User: {userId} Issuer: {issuer}", userId, issuer);

    public static void TokenValidationSucceeded(this ILogger logger, string userId, string issuer) =>
        logger.Debug("Token validation succeeded: User: {userId} Issuer: {issuer}", userId, issuer);

    public static void RegistrationDenied(this ILogger logger, string objectId, string reason) =>
        logger.Warning("Registration denied for ObjectId: {ObjectId}. Reason: {Reason}", objectId, reason);
}