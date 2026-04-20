using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wayd.Infrastructure.Auth.AzureAd;
using Wayd.Infrastructure.Auth.Entra;
using Wayd.Infrastructure.Auth.Local;
using Wayd.Infrastructure.Auth.Permissions;
using Wayd.Infrastructure.Auth.PersonalAccessToken;

namespace Wayd.Infrastructure.Auth;

internal static class ConfigureServices
{
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config, IHostEnvironment environment)
    {
        return services
            .AddCurrentUser()
            .AddPermissions()

            // Must add identity before adding auth!
            .AddIdentity()
            .AddAzureAdAuth(config)
            .AddLocalJwtAuth(config, environment)
            .AddEntraTokenExchange(config)
            .AddPersonalAccessTokenAuth()
            .AddAuthorizationPolicies();
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Default policy is dynamically determined by PermissionPolicyProvider
            // based on the presence of the x-api-key header to avoid unnecessary PAT authentication attempts
        });

        return services;
    }

    private static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
        services
            .AddHttpContextAccessor()
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

    private static IServiceCollection AddPermissions(this IServiceCollection services) =>
        services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()
            .AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
}