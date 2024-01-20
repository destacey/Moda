using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;

namespace Moda.Infrastructure.Middleware;

internal class ExceptionMiddleware : IMiddleware
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISerializerService _jsonSerializer;

    public ExceptionMiddleware(ICurrentUser currentUser, IDateTimeProvider dateTimeProvider, ISerializerService jsonSerializer)
    {
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _jsonSerializer = jsonSerializer;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        try
        {
            await next(httpContext);
        }
        catch (ValidationException ex)
        {
            string errorId = Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
            using (LogContext.PushProperty("RequestTimestampUtc", _dateTimeProvider.Now))
            using (LogContext.PushProperty("UserEmail", _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous"))
            using (LogContext.PushProperty("UserId", _currentUser.GetUserId()))
            using (LogContext.PushProperty("ErrorId", errorId))
            using (LogContext.PushProperty("ExceptionMessage", ex.Message))
            {
                var problemDetails = new ValidationProblemDetails(ex.Errors)
                {
                    Type = "https://developer.mozilla.org/en-US/docs/web/http/status/422",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = "See the errors property for details.",
                    Instance = httpContext.Request.Path
                };

                var response = httpContext.Response;
                response.ContentType = "application/problem+json";
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;

                var problemDetailsJson = _jsonSerializer.Serialize(problemDetails);
                using (LogContext.PushProperty("ProblemDetails", problemDetailsJson))
                {
                    Log.Information("Request failed with status code {StatusCode}", response.StatusCode);
                    await response.WriteAsync(problemDetailsJson);
                }
            }
        }
        catch (Exception ex)
        {
            string errorId = Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
            using (LogContext.PushProperty("RequestTimestampUtc", _dateTimeProvider.Now))
            using (LogContext.PushProperty("UserEmail", _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous"))
            using (LogContext.PushProperty("UserId", _currentUser.GetUserId()))
            using (LogContext.PushProperty("ErrorId", errorId))
            using (LogContext.PushProperty("ExceptionMessage", ex.Message))
            using (LogContext.PushProperty("StackTrace", ex.StackTrace))
            {
                var errorResult = new ErrorResult
                {
                    Source = ex.TargetSite?.DeclaringType?.FullName,
                    Exception = ex.Message.Trim(),
                    ErrorId = errorId,
                    SupportMessage = "exceptionmiddleware.supportmessage"
                };

                errorResult.Messages!.Add(ex.Message);

                var response = httpContext.Response;
                response.ContentType = "application/json";
                if (ex is not CustomException && ex.InnerException is not null)
                {
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }
                }

                switch (ex)
                {
                    case CustomException e:
                        response.StatusCode = errorResult.StatusCode = (int)e.StatusCode;
                        if (e.ErrorMessages is not null)
                        {
                            errorResult.Messages = e.ErrorMessages;
                        }
                        break;

                    case KeyNotFoundException:
                        response.StatusCode = errorResult.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    default:
                        response.StatusCode = errorResult.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                Log.Error("Request failed with Status Code {StatusCode}", response.StatusCode);
                await response.WriteAsync(_jsonSerializer.Serialize(errorResult));
            }
        }
    }
}