using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ApiGateway.Features.HealthChecks;

public class HealthCheckMiddleware
{
    private readonly ILogger<HealthCheckMiddleware>? _logger;
    private readonly HealthCheckService _healthCheckService;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly Action<ILogger, string, Exception?> LogHealthCheckRequest =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1001, nameof(LogHealthCheckRequest)),
            "Processing health check request for {Path}");

    private static readonly Action<ILogger, string, int, Exception?> LogHealthCheckResponse =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(1002, nameof(LogHealthCheckResponse)),
            "Health check response for {Path} returned status {StatusCode}");

    public HealthCheckMiddleware(ILogger<HealthCheckMiddleware>? logger, HealthCheckService healthCheckService)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        
        if (!IsHealthCheckEndpoint(path))
        {
            return;
        }

        if (_logger != null)
        {
            LogHealthCheckRequest(_logger, path!, null);
        }

        var response = await _healthCheckService.PerformHealthCheckAsync().ConfigureAwait(false);
        var statusCode = response.OverallStatus == "Healthy" ? 200 : 503;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(jsonResponse).ConfigureAwait(false);

        if (_logger != null)
        {
            LogHealthCheckResponse(_logger, path!, statusCode, null);
        }
    }

    private static bool IsHealthCheckEndpoint(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path.Equals("/health", StringComparison.OrdinalIgnoreCase) ||
               path.Equals("/ready", StringComparison.OrdinalIgnoreCase) ||
               path.Equals("/live", StringComparison.OrdinalIgnoreCase);
    }
}