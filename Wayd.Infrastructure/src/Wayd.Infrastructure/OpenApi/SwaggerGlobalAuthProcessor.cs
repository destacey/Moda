using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Wayd.Infrastructure.OpenApi;

internal static class ObjectExtensions
{
    public static T? TryGetPropertyValue<T>(this object? obj, string propertyName, T? defaultValue = default) =>
        obj?.GetType().GetRuntimeProperty(propertyName) is PropertyInfo propertyInfo
            ? (T?)propertyInfo.GetValue(obj)
            : defaultValue;
}

/// <summary>
/// The default NSwag AspNetCoreOperationProcessor doesn't take .RequireAuthorization() calls into account.
/// Unless the AllowAnonymous attribute is defined, this processor attaches the API Key (PAT) security
/// requirement to every operation, effectively adding "Global Auth" to the generated spec.
/// </summary>
public class SwaggerGlobalAuthProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        IList<object>? list = ((AspNetCoreOperationProcessorContext)context).ApiDescription?.ActionDescriptor?.TryGetPropertyValue<IList<object>>("EndpointMetadata");
        if (list is not null)
        {
            if (list.OfType<AllowAnonymousAttribute>().Any())
            {
                return true;
            }

            if (context.OperationDescription.Operation.Security?.Any() != true)
            {
                context.OperationDescription.Operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        { "ApiKey", Array.Empty<string>() }
                    }
                ];
            }
        }

        return true;
    }
}