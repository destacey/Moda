using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Moda.Infrastructure.Middleware;

internal sealed class ExceptionMiddleware(IProblemDetailsService problemDetailsService, ICurrentUser currentUser) : IMiddleware
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

                var problemDetails = CreateProblemDetails(httpContext, ex);

                var logLevel = GetLogLevelFromException(ex);
                Log.Write(logLevel, ex, "An unhandled exception has occurred while executing the request.");

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

    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => CreateValidationProblemDetails(httpContext, validationException),
            _ => new ProblemDetails
            {
                Title = "An error occurred while processing your request.",
                Detail = exception.Message
            }
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException exception)
    {
        return new ValidationProblemDetails(exception.Errors)
        {
            Title = "One or more validation errors occurred.",
            Detail = "See the errors property for details."
        };
    }
}