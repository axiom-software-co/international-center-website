using System.Text.Json;

namespace ApiGateway.Features.ErrorHandling;

public class ErrorResponseFormatter
{
    private readonly ErrorConfiguration _configuration;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    public ErrorResponseFormatter(ErrorConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // GREEN PHASE: Constructor for testing
    public ErrorResponseFormatter()
    {
        _configuration = new ErrorConfiguration();
    }
    
    public string FormatError(Exception exception, string? correlationId = null, string? environment = null)
    {
        // GREEN PHASE: Minimal implementation for medical-grade error formatting
        var errorResponse = new
        {
            error = new
            {
                message = GetErrorMessage(exception),
                type = exception.GetType().Name,
                correlationId = correlationId ?? Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow,
                details = ShouldIncludeDetails(environment) ? exception.Message : null,
                stackTrace = ShouldIncludeStackTrace(environment) ? exception.StackTrace : null
            }
        };
        
        return JsonSerializer.Serialize(errorResponse, JsonOptions);
    }
    
    public int GetStatusCodeForException(Exception exception)
    {
        // GREEN PHASE: Map exceptions to appropriate HTTP status codes
        var exceptionType = exception.GetType();
        
        if (_configuration.ExceptionStatusCodes.TryGetValue(exceptionType, out var statusCode))
            return statusCode;
        
        // Default to 500 for unhandled exceptions
        return 500;
    }
    
    private static string GetErrorMessage(Exception exception)
    {
        // GREEN PHASE: Provide user-friendly error messages
        return exception switch
        {
            ArgumentException => "Invalid request parameters",
            UnauthorizedAccessException => "Authentication required",
            InvalidOperationException => "Invalid operation",
            NotImplementedException => "Feature not implemented",
            TimeoutException => "Request timed out",
            _ => "An error occurred while processing your request"
        };
    }
    
    private bool ShouldIncludeDetails(string? environment)
    {
        if (!_configuration.IncludeDetails) return false;
        if (string.IsNullOrEmpty(environment)) return false;
        
        return _configuration.AllowedDetailEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool ShouldIncludeStackTrace(string? environment)
    {
        if (!_configuration.IncludeStackTrace) return false;
        if (string.IsNullOrEmpty(environment)) return false;
        
        return _configuration.AllowedDetailEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase);
    }
}
