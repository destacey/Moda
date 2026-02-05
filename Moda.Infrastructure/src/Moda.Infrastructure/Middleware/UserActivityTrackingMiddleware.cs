using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Moda.Infrastructure.Middleware;

public class UserActivityTrackingMiddleware(
    ICurrentUser currentUser,
    IMemoryCache memoryCache,
    UserActivityBackgroundService backgroundService) : IMiddleware
{
    private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly UserActivityBackgroundService _backgroundService = backgroundService;

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        if (_currentUser.IsAuthenticated())
        {
            TrackUserActivity();
        }

        await next(httpContext);
    }

    private void TrackUserActivity()
    {
        var userId = _currentUser.GetUserId();
        if (userId == Guid.Empty)
            return;

        var cacheKey = $"user-activity:{userId}";
        if (_memoryCache.TryGetValue(cacheKey, out _))
            return;

        // Set cache entry immediately to prevent duplicate writes from concurrent requests
        _memoryCache.Set(cacheKey, true, _cacheExpiration);

        // Queue the update for background processing
        _backgroundService.QueueUpdate(userId);
    }
}
