using Microsoft.Extensions.Logging;

namespace ApiGateway.Features.Observability;

public class RequestLoggingMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware>? _logger;
    private readonly ObservabilityConfiguration _configuration;

    private static readonly Action<ILogger, string, string, string, Exception?> LogRequestStarted =
        LoggerMessage.Define<string, string, string>(LogLevel.Information, new EventId(1001, nameof(LogRequestStarted)),
            "Request started {Method} {Path} - TraceId: {TraceId}");

    private static readonly Action<ILogger, string, string, int, string, Exception?> LogRequestCompleted =
        LoggerMessage.Define<string, string, int, string>(LogLevel.Information, new EventId(1002, nameof(LogRequestCompleted)),
            "Request completed {Method} {Path} - Status: {StatusCode} - TraceId: {TraceId}");

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware>? logger, ObservabilityConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_configuration.EnableRequestLogging)
        {
            return;
        }

        var correlationId = Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        if (_logger != null)
        {
            LogRequestStarted(_logger, context.Request.Method, context.Request.Path, context.TraceIdentifier, null);
        }

        await Task.CompletedTask.ConfigureAwait(false);

        if (_logger != null)
        {
            LogRequestCompleted(_logger, context.Request.Method, context.Request.Path, context.Response.StatusCode, context.TraceIdentifier, null);
        }
    }
}
