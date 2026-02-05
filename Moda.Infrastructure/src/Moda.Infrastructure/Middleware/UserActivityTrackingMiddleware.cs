using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Moda.Infrastructure.Middleware;

public class UserActivityTrackingMiddleware(
    ICurrentUser currentUser,
    IMemoryCache memoryCache,
    IServiceProvider serviceProvider,
    ILogger<UserActivityTrackingMiddleware> logger) : IMiddleware
{
    private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<UserActivityTrackingMiddleware> _logger = logger;

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

        // Fire-and-forget background update using a new scope to avoid tracking conflicts
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ModaDbContext>();
                var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId.ToString());
                if (user != null)
                {
                    user.LastActivityAt = dateTimeProvider.Now;
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update last activity for user {UserId}", userId);
            }
        });
    }
}
