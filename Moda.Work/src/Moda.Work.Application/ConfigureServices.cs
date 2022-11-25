using System.Reflection;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Moda.Work.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddWorkApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(assembly);

        TypeAdapterConfig.GlobalSettings.Scan(assembly);

        return services;
    }
}
