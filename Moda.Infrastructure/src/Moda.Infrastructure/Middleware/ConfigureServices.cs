using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Moda.Infrastructure.Middleware;

internal static class ConfigureServices
{
    internal static IServiceCollection AddExceptionMiddleware(this IServiceCollection services) =>
        services.AddScoped<ExceptionMiddleware>();

    internal static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionMiddleware>();

    internal static IServiceCollection AddRequestLogging(this IServiceCollection services, IConfiguration config)
    {
        if (GetMiddlewareSettings(config).EnableHttpsLogging)
        {
            services.AddScoped<RequestLoggingMiddleware>();
        }

        return services;
    }

    internal static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app, IConfiguration config)
    {
        if (GetMiddlewareSettings(config).EnableHttpsLogging)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseSerilogRequestLogging();
        }

        return app;
    }

    private static MiddlewareSettings GetMiddlewareSettings(IConfiguration config) =>
        config.GetSection(nameof(MiddlewareSettings)).Get<MiddlewareSettings>()!;
}