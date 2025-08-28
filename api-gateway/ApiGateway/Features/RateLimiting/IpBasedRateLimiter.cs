using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.RateLimiting;

public class IpBasedRateLimiter
{
    private readonly RateLimitConfiguration _configuration;
    private readonly RedisRateLimitStore _store;

    public IpBasedRateLimiter(RateLimitConfiguration configuration, RedisRateLimitStore store)
    {
        _configuration = configuration;
        _store = store;
    }

    public async Task<bool> IsAllowedAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress;
        if (ipAddress == null)
            return true;

        // Check bypass addresses
        if (_configuration.ShouldBypassRateLimit(ipAddress))
            return true;

        var key = $"ip:{ipAddress}";
        
        // Increment first, then check if we're over the limit
        await _store.IncrementRequestCountAsync(key, _configuration.WindowSize).ConfigureAwait(false);
        var currentCount = await _store.GetRequestCountAsync(key, _configuration.WindowSize).ConfigureAwait(false);
        
        return currentCount <= _configuration.DefaultRequestsPerMinute;
    }

    public async Task<RateLimitInfo> GetRateLimitInfoAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress;
        if (ipAddress == null)
        {
            return new RateLimitInfo
            {
                IsAllowed = true,
                RequestCount = 0,
                Limit = _configuration.DefaultRequestsPerMinute,
                ResetTime = _configuration.WindowSize
            };
        }

        var key = $"ip:{ipAddress}";
        var currentCount = await _store.GetRequestCountAsync(key, _configuration.WindowSize).ConfigureAwait(false);
        
        return new RateLimitInfo
        {
            IsAllowed = currentCount < _configuration.DefaultRequestsPerMinute,
            RequestCount = currentCount,
            Limit = _configuration.DefaultRequestsPerMinute,
            ResetTime = _configuration.WindowSize
        };
    }
}
