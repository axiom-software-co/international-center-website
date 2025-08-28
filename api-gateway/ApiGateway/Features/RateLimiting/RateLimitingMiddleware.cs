using Microsoft.Extensions.Logging;

namespace ApiGateway.Features.RateLimiting;

public class RateLimitingMiddleware
{
    private readonly ILogger<RateLimitingMiddleware>? _logger;
    private readonly IRateLimitingService _rateLimitingService;

    private static readonly Action<ILogger, string, Exception?> LogRateLimitExceeded =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1001, nameof(LogRateLimitExceeded)),
            "Rate limit exceeded for {ClientInfo}");

    public RateLimitingMiddleware(ILogger<RateLimitingMiddleware>? logger, IRateLimitingService rateLimitingService)
    {
        _logger = logger;
        _rateLimitingService = rateLimitingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isAllowed = await _rateLimitingService.IsAllowedAsync(context).ConfigureAwait(false);
        var rateLimitInfo = await _rateLimitingService.GetRateLimitInfoAsync(context).ConfigureAwait(false);

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = rateLimitInfo.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, rateLimitInfo.Limit - rateLimitInfo.RequestCount).ToString();
        
        if (!isAllowed)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = "60"; // Retry after 60 seconds
            
            if (_logger != null)
            {
                var clientInfo = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                LogRateLimitExceeded(_logger, clientInfo, null);
            }
            
            return;
        }

        context.Response.StatusCode = 200;
    }
}
