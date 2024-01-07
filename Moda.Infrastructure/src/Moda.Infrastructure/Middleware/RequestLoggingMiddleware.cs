using System.Text;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace Moda.Infrastructure.Middleware;

public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeManager;

    public RequestLoggingMiddleware(ICurrentUser currentUser, IDateTimeProvider dateTimeManager)
    {
        _currentUser = currentUser;
        _dateTimeManager = dateTimeManager;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        LogContext.PushProperty("RequestTimeUTC", _dateTimeManager.Now);
        string requestBody = string.Empty;
        if (httpContext.Request.Path.ToString().Contains("tokens"))
        {
            requestBody = "[Redacted] Contains Sensitive Information.";
        }
        else
        {
            var request = httpContext.Request;

            if (!string.IsNullOrEmpty(request.ContentType)
                && request.ContentType.StartsWith("application/json"))
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 4096, true);
                requestBody = await reader.ReadToEndAsync();

                // rewind for next middleware.
                request.Body.Position = 0;
            }
        }

        string email = _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous";
        var userId = _currentUser.GetUserId();

        if (userId != Guid.Empty)
            LogContext.PushProperty("UserId", userId);

        LogContext.PushProperty("UserEmail", email);

        LogContext.PushProperty("RequestBody", requestBody);
        Log.ForContext("RequestHeaders", httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
           .ForContext("RequestBody", requestBody)
           .Information("HTTP {RequestMethod} Request sent to {RequestPath}", httpContext.Request.Method, httpContext.Request.Path, string.IsNullOrEmpty(email) ? "Anonymous" : email);
        await next(httpContext);
    }
}