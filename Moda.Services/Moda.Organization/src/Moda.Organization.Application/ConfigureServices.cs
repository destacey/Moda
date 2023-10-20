using System.Reflection;
using Mapster;
using Mapster.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Moda.Organization.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddOrganizationApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(options => options.RegisterServicesFromAssembly(assembly));

        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        TypeAdapterConfig.GlobalSettings.ScanInheritedTypes(assembly);

        return services;
    }
}
