using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Moda.Infrastructure.Auth.Local;

internal static class ConfigureServices
{
    internal const string LocalJwtScheme = "LocalJwt";

    internal static IServiceCollection AddLocalJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetSection(LocalJwtSettings.SectionName).Get<LocalJwtSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{LocalJwtSettings.SectionName}' is missing.");

        if (string.IsNullOrWhiteSpace(settings.Secret))
        {
            throw new InvalidOperationException($"'{LocalJwtSettings.SectionName}:Secret' must be configured.");
        }

        var key = Encoding.UTF8.GetBytes(settings.Secret);

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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
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
}
