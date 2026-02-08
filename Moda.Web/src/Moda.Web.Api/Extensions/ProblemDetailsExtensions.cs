using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;
using System.Net;

namespace Moda.Web.Api.Extensions;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails ForBadRequest(string error, HttpContext context)
    {
        Activity? activity = context.Features.Get<IHttpActivityFeature>()?.Activity;
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = error,
            Instance = $"{context.Request.Method} {context.Request.Path}",
            Extensions =
                {
                    ["requestId"] = context.TraceIdentifier,
                    ["traceId"] = activity?.Id
                }
        };
        return problemDetails;
    }

    public static ProblemDetails ForConflict(string error, HttpContext context)
    {
        Activity? activity = context.Features.Get<IHttpActivityFeature>()?.Activity;
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Title = "Conflict",
            Status = (int)HttpStatusCode.Conflict,
            Detail = error,
            Instance = $"{context.Request.Method} {context.Request.Path}",
            Extensions =
                {
                    ["requestId"] = context.TraceIdentifier,
                    ["traceId"] = activity?.Id
                }
        };
        return problemDetails;
    }

    public static ProblemDetails ForUnknownIdOrKeyType(HttpContext context)
    {
        return ForBadRequest("Unknown id or key type.", context);
    }

    /// <summary>
    /// Returns a ProblemDetails object for a route parameter mismatch. The route parameter name is assumed to be "Id".
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static ProblemDetails ForRouteParamMismatch(HttpContext context)
    {
        return ForRouteParamMismatch("Id", "Id", context);
    }

    /// <summary>
    /// Returns a ProblemDetails object for a route parameter mismatch.
    /// </summary>
    /// <param name="routeParamName"></param>
    /// <param name="requestPropertyName"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static ProblemDetails ForRouteParamMismatch(string routeParamName, string requestPropertyName, HttpContext context)
    {
        string capitalizedRouteParamName = char.ToUpper(routeParamName[0]) + routeParamName.Substring(1);
        return ForBadRequest($"The route {capitalizedRouteParamName} and request {requestPropertyName} do not match.", context);
    }

}
