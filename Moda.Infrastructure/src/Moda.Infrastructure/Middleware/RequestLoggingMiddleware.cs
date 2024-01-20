using System.Text;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace Moda.Infrastructure.Middleware;

public class RequestLoggingMiddleware(ICurrentUser currentUser, IDateTimeProvider dateTimeProvider) : IMiddleware
{
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        using (LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
        using (LogContext.PushProperty("RequestTimeUtc", _dateTimeProvider.Now))
        using (LogContext.PushProperty("UserEmail", _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous"))
        using (LogContext.PushProperty("UserId", _currentUser.GetUserId()))
        {
            await next(httpContext);
        }
    }
}