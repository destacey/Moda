using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Moda.Planning.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddPlanningApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(options => options.RegisterServicesFromAssembly(assembly));

        TypeAdapterConfig.GlobalSettings.Scan(assembly);

        return services;
    }
}
