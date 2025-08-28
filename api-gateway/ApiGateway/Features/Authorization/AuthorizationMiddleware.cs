using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Features.Authorization;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate? _next;
    private readonly IEnumerable<IAuthorizationStrategy> _strategies;
    private readonly ILogger<AuthorizationMiddleware>? _logger;
    
    public AuthorizationMiddleware(RequestDelegate next, IEnumerable<IAuthorizationStrategy> strategies)
    {
        _next = next;
        _strategies = strategies;
    }
    
    // GREEN PHASE: Constructor for testing
    public AuthorizationMiddleware(ILogger<AuthorizationMiddleware> logger)
    {
        _logger = logger;
        _strategies = Array.Empty<IAuthorizationStrategy>();
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // GREEN PHASE: Minimal implementation for medical-grade authorization middleware
        var policy = context.Request.Headers["X-Authorization-Policy"].FirstOrDefault() ?? "Public";
        
        // Find appropriate authorization strategy
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(policy));
        if (strategy != null)
        {
            var result = await strategy.AuthorizeAsync(context.User, context, policy).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                context.Response.StatusCode = 403; // Forbidden
                return;
            }
        }
        
        // Continue to next middleware
        if (_next != null)
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}