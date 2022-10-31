using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Moda.Organization.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddOrganizationApplication(this IServiceCollection services)
    {
        MapsterSettings.Configure();

        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(assembly);

        return services;
    }
}
