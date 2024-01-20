using Microsoft.AspNetCore.Http;

namespace Moda.Infrastructure.Common.Services;
public class RequestCorrelationIdProvider(IHttpContextAccessor httpContextAccessor) : IRequestCorrelationIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Returns the TraceIdentifier from the HttpContext if it exists, otherwise null.
    /// </summary>
    public string? RequestCorrelationId => _httpContextAccessor.HttpContext?.TraceIdentifier;

    /// <summary>
    /// Returns the RequestCorrelationId if it exists, otherwise a new Guid is generated.
    /// </summary>
    public string CorrelationId => RequestCorrelationId ?? Guid.NewGuid().ToString();
}
