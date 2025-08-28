using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Features.Cors;

public class CorsMiddleware
{
    private readonly RequestDelegate? _next;
    private readonly ICorsService _corsService;
    private readonly ILogger<CorsMiddleware>? _logger;
    
    public CorsMiddleware(RequestDelegate next, ICorsService corsService)
    {
        _next = next;
        _corsService = corsService;
    }
    
    // GREEN PHASE: Constructor for testing
    public CorsMiddleware(ILogger<CorsMiddleware> logger, ICorsService corsService)
    {
        _logger = logger;
        _corsService = corsService;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // GREEN PHASE: Minimal implementation for medical-grade CORS middleware
        await _corsService.ApplyCorsAsync(context).ConfigureAwait(false);
        
        // For OPTIONS requests, we might have already set status to 204
        if (context.Response.StatusCode == 204)
            return;
        
        // Continue to next middleware
        if (_next != null)
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}
