using Moda.Web.BlazorClient.Infrastructure.Auth.AzureAd;
using Moda.Web.BlazorClient.Infrastructure.Auth.Graph;

namespace Moda.Web.BlazorClient.Infrastructure.Auth;

internal static class ConfigureServices
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config) =>
        config[nameof(AuthProvider)] switch
        {
            // AzureAd
            nameof(AuthProvider.AzureAd) => services
                .AddScoped<IAuthenticationService, AzureAdAuthenticationService>()
                .AddScoped<AzureAdAuthorizationMessageHandler>()
                .AddScoped<GraphAPIAuthorizationMessageHandler>()
                .AddMsalAuthentication(options =>
                {
                    config.Bind(nameof(AuthProvider.AzureAd), options.ProviderOptions.Authentication);
#pragma warning disable CS8604 // Possible null reference argument.
                    options.ProviderOptions.DefaultAccessTokenScopes?.Add(
                        config[$"{nameof(AuthProvider.AzureAd)}:{ConfigNames.ApiScope}"]);
                    options.ProviderOptions.DefaultAccessTokenScopes?.Add("User.Read");
#pragma warning restore CS8604 // Possible null reference argument.
                    options.ProviderOptions.LoginMode = "redirect";
                    options.ProviderOptions.Cache.CacheLocation = "localStorage";
                })
                    .AddAccountClaimsPrincipalFactory<AzureAdClaimsPrincipalFactory>()
                    .Services,
            _ => throw new NotImplementedException()
        };

    public static IHttpClientBuilder AddAuthenticationHandler(this IHttpClientBuilder builder, IConfiguration config) =>
        config[nameof(AuthProvider)] switch
        {
            // AzureAd
            nameof(AuthProvider.AzureAd) =>
                builder.AddHttpMessageHandler<AzureAdAuthorizationMessageHandler>(),
            _ => throw new NotImplementedException()
        };
}
