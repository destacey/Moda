﻿using System.Diagnostics;
using System.Reflection;
using Asp.Versioning;
using Mapster;
using Mapster.Utils;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moda.Integrations.AzureDevOps;
using Moda.Integrations.MicrosoftGraph;
using NodaTime;

namespace Moda.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var assembly = Assembly.GetExecutingAssembly();
        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        TypeAdapterConfig.GlobalSettings.ScanInheritedTypes(assembly);

        services.AddSingleton<IClock>(SystemClock.Instance);

        // INTEGRATIONS
        services.AddTransient<IAzureDevOpsService, AzureDevOpsService>();
        services.AddScoped<IExternalEmployeeDirectoryService, MicrosoftGraphService>();
        services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = true; });

        return services
            .AddApiVersioning()
            .AddAuth(config)
            .AddBackgroundJobs(config)
            .AddCorsPolicy(config)
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = (context) =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                    Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
                };
            })
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
        }).Services;

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
            //.UseStatusCodePages()
            .UseExceptionMiddleware()
            //.UseHttpsRedirection() // TODO: we don't currently need this because we are using docker.  Add a config setting to enable this when needed.
            .UseRouting()
            .UseCorsPolicy()
            .UseAuthentication()
            .UseCurrentUser()
            .UseAuthorization()
            .UseRequestLogging(config) // TODO: we currently don't log 403 logs because it is lower in the middleware pipeline. It should be above UseRouting, but then we don't get user information.
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
