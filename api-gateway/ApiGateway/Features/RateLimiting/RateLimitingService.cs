using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.RateLimiting;

public class RateLimitingService : IRateLimitingService
{
    private readonly RateLimitConfiguration _configuration;
    private readonly IpBasedRateLimiter _ipLimiter;
    private readonly UserBasedRateLimiter _userLimiter;

    public RateLimitingService(
        RateLimitConfiguration configuration,
        IpBasedRateLimiter ipLimiter,
        UserBasedRateLimiter userLimiter)
    {
        _configuration = configuration;
        _ipLimiter = ipLimiter;
        _userLimiter = userLimiter;
    }

    public async Task<bool> IsAllowedAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // Check IP-based rate limiting first
        if (_configuration.EnableIpBasedLimiting)
        {
            var ipAllowed = await _ipLimiter.IsAllowedAsync(context).ConfigureAwait(false);
            if (!ipAllowed)
                return false;
        }

        // Then check user-based rate limiting
        if (_configuration.EnableUserBasedLimiting)
        {
            var userAllowed = await _userLimiter.IsAllowedAsync(context).ConfigureAwait(false);
            if (!userAllowed)
                return false;
        }

        return true;
    }

    public async Task IncrementRequestCountAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // This is handled within the IsAllowedAsync methods of the limiters
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task<RateLimitInfo> GetRateLimitInfoAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // Get info from IP limiter (primary rate limiting mechanism)
        var info = await _ipLimiter.GetRateLimitInfoAsync(context).ConfigureAwait(false);
        
        // Also check user-based limiting if enabled
        if (_configuration.EnableUserBasedLimiting)
        {
            var userInfo = await _userLimiter.GetRateLimitInfoAsync(context).ConfigureAwait(false);
            
            // Return the more restrictive limit
            if (userInfo.Limit < info.Limit)
            {
                return userInfo;
            }
        }

        return info;
    }
}
