using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moda.Integrations.MicrosoftGraph;
using Moda.Organization.Application.Interfaces;
using NodaTime;

namespace Moda.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        //MapsterSettings.Configure();

        services.AddSingleton<IClock>(SystemClock.Instance);

        // INTEGRATIONS
        services.AddScoped<IExternalEmployeeDirectoryService, MicrosoftGraphService>();

        return services
            .AddApiVersioning()
            .AddAuth(config)
            .AddBackgroundJobs(config)
            .AddCorsPolicy(config)
            .AddExceptionMiddleware()
            .AddHealthCheck()
            .AddMediatR(options => options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddOpenApiDocumentation(config)
            .AddPersistence(config)
            .AddRequestLogging(config)
            .AddApplicationInsightsTelemetry()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddServices();
    }

    private static IServiceCollection AddApiVersioning(this IServiceCollection services) =>
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

    private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
        services.AddHealthChecks().Services;

    public static async Task InitializeDatabases(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabase(cancellationToken);
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
        builder
            .UseStaticFiles()
            .UseSecurityHeaders(config)
            .UseExceptionMiddleware()
            .UseHttpsRedirection()
            .UseRouting()
            .UseCorsPolicy()
            .UseAuthentication()
            .UseCurrentUser()
            .UseAuthorization()
            .UseRequestLogging(config)
            .UseHangfireDashboard(config)
            .UseOpenApiDocumentation(config);

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers().RequireAuthorization();
        builder.MapHealthCheck();
        return builder;
    }

    private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("/api/health").RequireAuthorization();
}
