using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.Cors;

public class CorsService : ICorsService
{
    private readonly PublicCorsPolicy _publicPolicy;
    private readonly AdminCorsPolicy _adminPolicy;
    
    public CorsService()
    {
        _publicPolicy = new PublicCorsPolicy();
        _adminPolicy = new AdminCorsPolicy();
    }
    
    // GREEN PHASE: Constructor for testing
    public CorsService(PublicCorsPolicy publicPolicy, AdminCorsPolicy adminPolicy)
    {
        _publicPolicy = publicPolicy;
        _adminPolicy = adminPolicy;
    }
    
    public Task ApplyCorsAsync(HttpContext context)
    {
        // GREEN PHASE: Minimal implementation for medical-grade CORS handling
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        if (string.IsNullOrEmpty(origin))
            return Task.CompletedTask;
        
        // Determine policy based on request path
        var policy = GetPolicyForRequest(context);
        var config = policy.GetConfiguration();
        
        if (policy.IsOriginAllowed(origin))
        {
            // Set CORS headers
            context.Response.Headers.AccessControlAllowOrigin = origin;
            context.Response.Headers.AccessControlAllowCredentials = config.AllowCredentials.ToString().ToLowerInvariant();
            context.Response.Headers.AccessControlMaxAge = config.MaxAge.ToString();
            
            // Handle preflight requests
            if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers.AccessControlAllowMethods = string.Join(", ", config.AllowedMethods);
                context.Response.Headers.AccessControlAllowHeaders = string.Join(", ", config.AllowedHeaders);
                context.Response.StatusCode = 204; // No Content
            }
        }
        
        return Task.CompletedTask;
    }
    
    public bool IsOriginAllowed(string origin)
    {
        // GREEN PHASE: Check both public and admin policies
        return _publicPolicy.IsOriginAllowed(origin) || _adminPolicy.IsOriginAllowed(origin);
    }
    
    private ICorsPolicy GetPolicyForRequest(HttpContext context)
    {
        // GREEN PHASE: Simple path-based policy selection
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        
        if (path.StartsWith("/admin"))
            return _adminPolicy;
            
        return _publicPolicy;
    }
}