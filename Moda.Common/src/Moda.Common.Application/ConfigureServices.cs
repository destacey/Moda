using System.Reflection;
using Mapster;
using Mapster.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Moda.Common.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddCommonApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        TypeAdapterConfig.GlobalSettings.ScanInheritedTypes(assembly);

        return services;
    }
}
