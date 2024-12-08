using Microsoft.AspNetCore.Http;

namespace Moda.Infrastructure.Middleware;
public sealed class StatusCodeManagerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            var statusCode = ExceptionMiddleware.GetStatusCodeFromException(ex);
            httpContext.Response.StatusCode = statusCode;

            throw;
        }
    }
}
