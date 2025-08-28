using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiGateway.Features.Security;

public class ResponseSecurityMiddleware
{
    private readonly SecurityConfiguration _configuration;
    private readonly ILogger<ResponseSecurityMiddleware> _logger;
    
    private static readonly Action<ILogger, string, Exception?> LogSecuringResponse =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(3003, nameof(LogSecuringResponse)), 
            "Securing response for request {Path}");

    public ResponseSecurityMiddleware(SecurityConfiguration configuration, ILogger<ResponseSecurityMiddleware> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public ResponseSecurityMiddleware(SecurityConfiguration configuration)
        : this(configuration, new NullLogger<ResponseSecurityMiddleware>())
    {
    }

    public async Task ProcessRequestAsync(HttpContext context, RequestDelegate next)
    {
        LogSecuringResponse(_logger, context.Request.Path, null);
        
        await next(context).ConfigureAwait(false);
        
        SecureResponse(context);
    }

    private void SecureResponse(HttpContext context)
    {
        var headers = context.Response.Headers;
        
        // Remove server information headers
        headers.Remove("X-Powered-By");
        headers.Remove("Server");
        headers.Remove("X-AspNet-Version");
        headers.Remove("X-AspNetMvc-Version");
        
        // Ensure content type is properly set for security
        if (!headers.ContainsKey("Content-Type") && context.Response.Body.Length > 0)
        {
            headers["Content-Type"] = "application/json; charset=utf-8";
        }
        
        // Add cache control for sensitive responses
        if (IsSensitivePath(context.Request.Path))
        {
            headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";
        }
    }

    private static bool IsSensitivePath(PathString path)
    {
        var sensitivePaths = new[] { "/admin", "/api/admin", "/health", "/metrics" };
        return sensitivePaths.Any(sensitive => 
            path.StartsWithSegments(sensitive, StringComparison.OrdinalIgnoreCase));
    }
}