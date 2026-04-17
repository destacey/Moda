using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Wayd.Infrastructure.GraphApi;
using Wayd.Integrations.MicrosoftGraph;
using Serilog;

namespace Wayd.Infrastructure.Auth.AzureAd;

internal static class ConfigureServices
{
    internal static IServiceCollection AddAzureAdAuth(this IServiceCollection services, IConfiguration config)
    {
        var logger = Log.ForContext(typeof(AzureAdJwtBearerEvents));

        services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddMicrosoftIdentityWebApi(
                 jwtOptions => jwtOptions.Events = new AzureAdJwtBearerEvents(logger, config),
                 msIdentityOptions => config.GetSection(AzureAdSettings.SectionName).Bind(msIdentityOptions))
            .EnableTokenAcquisitionToCallDownstreamApi(clientAppOptions => config.GetSection(AzureAdSettings.SectionName).Bind(clientAppOptions))
            .AddInMemoryTokenCaches();

        services.AddScoped(provider =>
        {
            var azureAdSettings = AzureAdSettings.GetConfig(config);
            GraphHelper.InitializeGraphForAppOnlyAuth(azureAdSettings);
            return GraphHelper.GetAppOnlyClient();
        });

        services.AddScoped<MicrosoftGraphService>();

        return services;
    }
}