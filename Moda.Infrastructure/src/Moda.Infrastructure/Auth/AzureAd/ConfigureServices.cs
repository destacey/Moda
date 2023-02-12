using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moda.Integrations.MicrosoftGraph;
using Serilog;

namespace Moda.Infrastructure.Auth.AzureAd;

internal static class ConfigureServices
{
    internal static IServiceCollection AddAzureAdAuth(this IServiceCollection services, IConfiguration config)
    {
        var logger = Log.ForContext(typeof(AzureAdJwtBearerEvents));

        services
            .AddAuthorization()
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddMicrosoftIdentityWebApi(
                 jwtOptions => jwtOptions.Events = new AzureAdJwtBearerEvents(logger, config),
                 msIdentityOptions => config.GetSection("SecuritySettings:AzureAd").Bind(msIdentityOptions))
            .EnableTokenAcquisitionToCallDownstreamApi(clientAppOptions => config.GetSection("SecuritySettings:AzureAd").Bind(clientAppOptions))
            //.EnableTokenAcquisitionToCallDownstreamApi(confidentialClientApplicationOptions =>
            //{
            //    confidentialClientApplicationOptions.ClientId = config.GetSection("SecuritySettings:AzureAd").GetValue<string>("ClientId");
            //    confidentialClientApplicationOptions.ClientSecret = config.GetSection("SecuritySettings:AzureAd").GetValue<string>("ClientSecret");
            //    confidentialClientApplicationOptions.TenantId = config.GetSection("SecuritySettings:AzureAd").GetValue<string>("TenantId");
            //})
            //.AddMicrosoftGraph(config.GetSection("SecuritySettings:GraphApi"))
            .AddInMemoryTokenCaches();

        services.AddScoped(provider =>
        {
            string[] scopes = { "https://graph.microsoft.com/.default" };
            var configuration = provider.GetRequiredService<IConfiguration>();
            var httpContext = provider.GetRequiredService<IHttpContextAccessor>();
            //var token = (httpContext?.HttpContext?.User?.Identity as ClaimsIdentity)
            //    ?.FindFirst("jwt")?.Value;

            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(configuration["SecuritySettings:AzureAd:ClientId"])
                .WithTenantId(configuration["SecuritySettings:AzureAd:TenantId"])
                .WithClientSecret(configuration["SecuritySettings:AzureAd:ClientSecret"] ?? "")
                .Build();

            return new GraphServiceClient(new DelegateAuthenticationProvider(async message =>
            {
                //var assertion = new UserAssertion(token);
                var authResult = await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync();
                //.AcquireTokenOnBehalfOf(scopes, assertion).ExecuteAsync();

                message.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            }));
        });

        services.AddScoped<MicrosoftGraphService>();

        return services;
    }
}