using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.RateLimiting;

public class UserBasedRateLimiter
{
    private readonly RateLimitConfiguration _configuration;
    private readonly RedisRateLimitStore _store;

    public UserBasedRateLimiter(RateLimitConfiguration configuration, RedisRateLimitStore store)
    {
        _configuration = configuration;
        _store = store;
    }

    public async Task<bool> IsAllowedAsync(HttpContext context)
    {
        var userId = GetUserId(context);
        if (string.IsNullOrEmpty(userId))
            return true;

        var isAdminUser = IsAdminUser(context);
        var limit = isAdminUser ? _configuration.AdminRequestsPerMinute : _configuration.DefaultRequestsPerMinute;
        
        var key = $"user:{userId}";
        
        // Increment first, then check if we're over the limit
        await _store.IncrementRequestCountAsync(key, _configuration.WindowSize).ConfigureAwait(false);
        var currentCount = await _store.GetRequestCountAsync(key, _configuration.WindowSize).ConfigureAwait(false);
        
        return currentCount <= limit;
    }

    public async Task<RateLimitInfo> GetRateLimitInfoAsync(HttpContext context)
    {
        var userId = GetUserId(context);
        if (string.IsNullOrEmpty(userId))
        {
            return new RateLimitInfo
            {
                IsAllowed = true,
                RequestCount = 0,
                Limit = _configuration.DefaultRequestsPerMinute,
                ResetTime = _configuration.WindowSize
            };
        }

        var isAdminUser = IsAdminUser(context);
        var limit = isAdminUser ? _configuration.AdminRequestsPerMinute : _configuration.DefaultRequestsPerMinute;
        
        var key = $"user:{userId}";
        var currentCount = await _store.GetRequestCountAsync(key, _configuration.WindowSize).ConfigureAwait(false);
        
        return new RateLimitInfo
        {
            IsAllowed = currentCount < limit,
            RequestCount = currentCount,
            Limit = limit,
            ResetTime = _configuration.WindowSize
        };
    }

    private static string? GetUserId(HttpContext context)
    {
        return context.Request.Headers["X-User-ID"].FirstOrDefault() ??
               context.User?.Identity?.Name;
    }

    private static bool IsAdminUser(HttpContext context)
    {
        var userId = GetUserId(context);
        return !string.IsNullOrEmpty(userId) && userId.Contains("admin", StringComparison.OrdinalIgnoreCase);
    }
}
