using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Moda.Infrastructure.Middleware;

public sealed class ExceptionMiddleware(IProblemDetailsService problemDetailsService, ICurrentUser currentUser) : IMiddleware
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {            
            using (LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
            using (LogContext.PushProperty("UserEmail", _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous"))
            using (LogContext.PushProperty("UserId", _currentUser.GetUserId()))
            using (LogContext.PushProperty("ExceptionMessage", ex.Message))
            using (LogContext.PushProperty("SourceContext", $"{typeof(ExceptionMiddleware).FullName}"))
            {

                httpContext.Response.ContentType = "application/problem+json";
                httpContext.Response.StatusCode = GetStatusCodeFromException(ex);

                if (ex is not CustomException && ex.InnerException is not null)
                {
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }
                }

                var logLevel = GetLogLevelFromException(ex);
                Log.Write(logLevel, ex, "An unhandled exception has occurred while executing the request.");

                if (ex is ValidationException validationException)
                {
                    var validationProblemDetails = CreateValidationProblemDetails(validationException, httpContext);

                    var json = System.Text.Json.JsonSerializer.Serialize(validationProblemDetails);
                    await httpContext.Response.WriteAsync(json);

                    return;
                }

                var problemDetails = new ProblemDetails
                {
                    Title = "An error occurred while processing your request.",
                    Detail = ex.Message
                };

                await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = ex,
                    ProblemDetails = problemDetails
                });
            }
        }
    }

    public static int GetStatusCodeFromException(Exception exception)
    {
        return exception switch
        {
            ApplicationException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status418ImATeapot
        };
    }
    public static LogEventLevel GetLogLevelFromException(Exception exception)
    {
        return exception switch
        {
            ApplicationException => LogEventLevel.Error,
            UnauthorizedException => LogEventLevel.Warning,
            ForbiddenException => LogEventLevel.Warning,
            NotFoundException => LogEventLevel.Information,
            ConflictException => LogEventLevel.Warning,
            ValidationException => LogEventLevel.Information,
            _ => LogEventLevel.Error
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(ValidationException exception, HttpContext httpContext)
    {
        return EnrichValidationProblemDetails(new ValidationProblemDetails(exception.Errors), httpContext);
    }

    public static ValidationProblemDetails EnrichValidationProblemDetails(ValidationProblemDetails validationProblemDetails, HttpContext httpContext)
    {
        Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;

        validationProblemDetails.Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
        validationProblemDetails.Title = "One or more validation errors occurred.";
        validationProblemDetails.Status = StatusCodes.Status422UnprocessableEntity;
        validationProblemDetails.Detail = "See the errors property for details.";
        validationProblemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        validationProblemDetails.Extensions = new Dictionary<string, object?>
        {
            ["requestId"] = httpContext.TraceIdentifier,
            ["traceId"] = activity?.Id
        };

        return validationProblemDetails;
    }
}