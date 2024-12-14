using System.Diagnostics;
using System.Net;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http.Features;

namespace Moda.Web.Api.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToBadRequest(this Result result, HttpContext context)
    {
        Activity? activity = context.Features.Get<IHttpActivityFeature>()?.Activity;

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = result.Error,
            Instance = $"{context.Request.Method} {context.Request.Path}",
            Extensions =
                {
                    ["requestId"] = context.TraceIdentifier,
                    ["traceId"] = activity?.Id
                }
        };

        return problemDetails;
    }

    public static ProblemDetails ToNotFound(this Result result, HttpContext context)
    {
        Activity? activity = context.Features.Get<IHttpActivityFeature>()?.Activity;

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Status = (int)HttpStatusCode.NotFound,
            Instance = $"{context.Request.Method} {context.Request.Path}",
            Extensions =
                {
                    ["requestId"] = context.TraceIdentifier,
                    ["traceId"] = activity?.Id
                }
        };

        return problemDetails;
    }
}
