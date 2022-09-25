using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Moda.Core.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddCoreApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(assembly);

        return services;
    }
}
