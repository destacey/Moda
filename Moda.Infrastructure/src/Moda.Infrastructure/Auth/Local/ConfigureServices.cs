using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Moda.Infrastructure.Auth.Local;

internal static class ConfigureServices
{
    internal const string LocalJwtScheme = "LocalJwt";

    private const string DevOnlySecretPrefix = "DEV-ONLY-SECRET";

    internal static IServiceCollection AddLocalJwtAuth(this IServiceCollection services, IConfiguration config, IHostEnvironment environment)
    {
        var settings = config.GetSection(LocalJwtSettings.SectionName).Get<LocalJwtSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{LocalJwtSettings.SectionName}' is missing.");

        if (string.IsNullOrWhiteSpace(settings.Secret))
        {
            throw new InvalidOperationException($"'{LocalJwtSettings.SectionName}:Secret' must be configured.");
        }

        if (!environment.IsDevelopment() && settings.Secret.StartsWith(DevOnlySecretPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"'{LocalJwtSettings.SectionName}:Secret' contains a development-only placeholder. "
                + "Configure a secure secret via user-secrets or environment variables for non-development environments.");
        }

        var signingKey = CreateSigningKey(settings.Secret);

        services.AddAuthentication()
            .AddJwtBearer(LocalJwtScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Support SignalR WebSocket token passing
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    internal static SymmetricSecurityKey CreateSigningKey(string secret)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        key.KeyId = Convert.ToBase64String(SHA256.HashData(key.Key))[..8];
        return key;
    }
}
