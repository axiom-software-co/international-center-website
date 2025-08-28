using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Features.ErrorHandling;

public class GatewayErrorHandler
{
    private readonly ErrorResponseFormatter _formatter;
    private readonly ILogger<GatewayErrorHandler>? _logger;
    
    private static readonly Action<ILogger, string, Exception?> LogGatewayError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(2001, nameof(LogGatewayError)), 
            "Gateway error occurred. CorrelationId: {CorrelationId}");
    
    public GatewayErrorHandler(ErrorResponseFormatter formatter)
    {
        _formatter = formatter;
    }
    
    // GREEN PHASE: Constructor for testing
    public GatewayErrorHandler(ILogger<GatewayErrorHandler> logger, ErrorResponseFormatter formatter)
    {
        _logger = logger;
        _formatter = formatter;
    }
    
    public async Task HandleErrorAsync(HttpContext context, Exception exception)
    {
        // GREEN PHASE: Minimal implementation for medical-grade error handling
        var correlationId = context.TraceIdentifier;
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        
        // Log the error
        if (_logger != null)
        {
            LogGatewayError(_logger, correlationId, exception);
        }
        
        // Set response properties
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = _formatter.GetStatusCodeForException(exception);
        
        // Add correlation ID header for tracing
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        // Format and write error response
        var errorJson = _formatter.FormatError(exception, correlationId, environment);
        await context.Response.WriteAsync(errorJson).ConfigureAwait(false);
    }
    
    public bool ShouldRetry(Exception exception, int attemptCount, int maxAttempts)
    {
        // GREEN PHASE: Simple retry logic for transient failures
        if (attemptCount >= maxAttempts)
            return false;
            
        return exception switch
        {
            TimeoutException => true,
            HttpRequestException => true,
            TaskCanceledException => true,
            _ => false
        };
    }
}
