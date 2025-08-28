using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ApiGateway.Features.Observability;

public class MetricsCollectionMiddleware
{
    private readonly ILogger<MetricsCollectionMiddleware>? _logger;
    private readonly GatewayMetrics _metrics;

    private static readonly Action<ILogger, string, long, Exception?> LogMetricsCollected =
        LoggerMessage.Define<string, long>(LogLevel.Debug, new EventId(1001, nameof(LogMetricsCollected)),
            "Metrics collected for {Path} - Response time: {ResponseTimeMs}ms");

    public MetricsCollectionMiddleware(ILogger<MetricsCollectionMiddleware>? logger, GatewayMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _metrics.IncrementTotalRequests();

        await Task.CompletedTask.ConfigureAwait(false);
        
        stopwatch.Stop();
        var responseTime = stopwatch.Elapsed;
        
        _metrics.RecordResponseTime(responseTime);
        
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            _metrics.IncrementSuccessfulRequests();
        }
        else
        {
            _metrics.IncrementFailedRequests();
        }

        context.Response.Headers["X-Response-Time"] = $"{responseTime.TotalMilliseconds}ms";

        if (_logger != null)
        {
            LogMetricsCollected(_logger, context.Request.Path, (long)responseTime.TotalMilliseconds, null);
        }
    }
}
