using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Features.Authentication;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate? _next;
    private readonly IEnumerable<IAuthenticationStrategy> _strategies;
    private readonly ILogger<AuthenticationMiddleware>? _logger;
    
    private static readonly Action<ILogger, string, Exception?> LogProcessingAuthentication =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1001, nameof(LogProcessingAuthentication)), 
            "Processing authentication for request {Path}");
            
    private static readonly Action<ILogger, string?, string, Exception?> LogAuthenticatedUser =
        LoggerMessage.Define<string?, string>(LogLevel.Information, new EventId(1002, nameof(LogAuthenticatedUser)), 
            "Authenticated user {User} via {Strategy}");
    
    public AuthenticationMiddleware(RequestDelegate next, IEnumerable<IAuthenticationStrategy> strategies)
    {
        _next = next;
        _strategies = strategies;
    }
    
    // GREEN PHASE: Constructor for testing
    public AuthenticationMiddleware(ILogger<AuthenticationMiddleware> logger)
    {
        _logger = logger;
        _strategies = Array.Empty<IAuthenticationStrategy>();
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // GREEN PHASE: Minimal implementation for medical-grade authentication middleware
        if (_logger != null)
        {
            LogProcessingAuthentication(_logger, context.Request.Path, null);
        }
        
        // Find appropriate authentication strategy
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(context));
        if (strategy != null)
        {
            var principal = await strategy.AuthenticateAsync(context).ConfigureAwait(false);
            if (principal != null)
            {
                context.User = principal;
                if (_logger != null)
                {
                    LogAuthenticatedUser(_logger, principal.Identity?.Name, strategy.Name, null);
                }
            }
        }
        
        // Continue to next middleware
        if (_next != null)
        {
            await _next(context).ConfigureAwait(false);
        }
        else if (next != null)
        {
            await next(context).ConfigureAwait(false);
        }
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        await InvokeAsync(context, _next!).ConfigureAwait(false);
    }
}