using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Moda.Infrastructure.Auth.PersonalAccessToken;

internal static class ConfigureServices
{
    internal static IServiceCollection AddPersonalAccessTokenAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, PersonalAccessTokenAuthenticationHandler>(
                "PersonalAccessToken",
                options => { });

        return services;
    }
}
