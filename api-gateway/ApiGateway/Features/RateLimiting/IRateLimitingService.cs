using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.RateLimiting;

public interface IRateLimitingService
{
    Task<bool> IsAllowedAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task IncrementRequestCountAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task<RateLimitInfo> GetRateLimitInfoAsync(HttpContext context, CancellationToken cancellationToken = default);
}

public class RateLimitInfo
{
    public bool IsAllowed { get; set; }
    public int RequestCount { get; set; }
    public int Limit { get; set; }
    public TimeSpan ResetTime { get; set; }
}