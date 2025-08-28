using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiGateway.Features.Security;

public class RequestValidationMiddleware
{
    private readonly SecurityConfiguration _configuration;
    private readonly ILogger<RequestValidationMiddleware> _logger;
    
    private static readonly Action<ILogger, string, Exception?> LogValidatingRequest =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(3002, nameof(LogValidatingRequest)), 
            "Validating request {Path}");

    public RequestValidationMiddleware(SecurityConfiguration configuration, ILogger<RequestValidationMiddleware> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public RequestValidationMiddleware(SecurityConfiguration configuration)
        : this(configuration, new NullLogger<RequestValidationMiddleware>())
    {
    }

    public async Task ProcessRequestAsync(HttpContext context, RequestDelegate next)
    {
        LogValidatingRequest(_logger, context.Request.Path, null);
        
        if (_configuration.EnableRequestValidation)
        {
            var validationResult = ValidateRequest(context);
            if (!validationResult.IsValid)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Bad Request").ConfigureAwait(false);
                return;
            }
        }
        
        await next(context).ConfigureAwait(false);
    }

    private ValidationResult ValidateRequest(HttpContext context)
    {
        // Validate HTTP method
        if (!_configuration.AllowedMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Invalid("Method not allowed");
        }

        // Validate User-Agent header
        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (_configuration.BlockedUserAgents.Any(blocked => 
            userAgent.Contains(blocked, StringComparison.OrdinalIgnoreCase)))
        {
            return ValidationResult.Invalid("Blocked user agent");
        }

        // Basic XSS protection in headers
        foreach (var header in context.Request.Headers)
        {
            if (ContainsSuspiciousContent(header.Value.ToString()))
            {
                return ValidationResult.Invalid("Suspicious header content");
            }
        }

        return ValidationResult.Valid();
    }

    private static bool ContainsSuspiciousContent(string content)
    {
        var suspiciousPatterns = new[] { "<script", "javascript:", "vbscript:", "onload=", "onerror=" };
        return suspiciousPatterns.Any(pattern => 
            content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Valid() => new(true);
    public static ValidationResult Invalid(string errorMessage) => new(false, errorMessage);
}