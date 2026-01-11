using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moda.Infrastructure.Auth.AzureAd;
using Moda.Infrastructure.Auth.Permissions;
using Moda.Infrastructure.Auth.PersonalAccessToken;

namespace Moda.Infrastructure.Auth;

internal static class ConfigureServices
{
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddCurrentUser()
            .AddPermissions()

            // Must add identity before adding auth!
            .AddIdentity()
            .AddAzureAdAuth(config)
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
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
}