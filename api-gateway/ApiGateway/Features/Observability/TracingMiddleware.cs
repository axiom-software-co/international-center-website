using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ApiGateway.Features.Observability;

public class TracingMiddleware
{
    private readonly ILogger<TracingMiddleware>? _logger;
    private readonly ObservabilityConfiguration _configuration;

    private static readonly Action<ILogger, string, string, string, Exception?> LogTraceStarted =
        LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId(1001, nameof(LogTraceStarted)),
            "Trace started for {Method} {Path} - TraceId: {TraceId}");

    public TracingMiddleware(ILogger<TracingMiddleware>? logger, ObservabilityConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_configuration.EnableTracing)
        {
            return;
        }

        var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();

        context.Response.Headers["X-Trace-ID"] = traceId;
        context.Response.Headers["X-Request-ID"] = requestId;

        if (_logger != null)
        {
            LogTraceStarted(_logger, context.Request.Method, context.Request.Path, traceId, null);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
