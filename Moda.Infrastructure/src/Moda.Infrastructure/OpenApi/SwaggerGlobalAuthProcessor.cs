using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Moda.Infrastructure.OpenApi;

internal static class ObjectExtensions
{
    public static T? TryGetPropertyValue<T>(this object? obj, string propertyName, T? defaultValue = default) =>
        obj?.GetType().GetRuntimeProperty(propertyName) is PropertyInfo propertyInfo
            ? (T?)propertyInfo.GetValue(obj)
            : defaultValue;
}

/// <summary>
/// The default NSwag AspNetCoreOperationProcessor doesn't take .RequireAuthorization() calls into account
/// Unless the AllowAnonymous attribute is defined, this processor will always add the security schemes
/// when they're not already there, so effectively adding "Global Auth".
/// Supports multiple authentication schemes (JWT Bearer and API Key).
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
                // Add both authentication schemes as alternatives
                // Each security requirement in the array represents an OR relationship
                // Users can authenticate with either JWT Bearer OR API Key
                context.OperationDescription.Operation.Security = new List<OpenApiSecurityRequirement>
                {
                    // Option 1: JWT Bearer (OAuth2)
                    new OpenApiSecurityRequirement
                    {
                        { JwtBearerDefaults.AuthenticationScheme, Array.Empty<string>() }
                    },
                    // Option 2: API Key (Personal Access Token)
                    new OpenApiSecurityRequirement
                    {
                        { "ApiKey", Array.Empty<string>() }
                    }
                };
            }
        }

        return true;
    }
}