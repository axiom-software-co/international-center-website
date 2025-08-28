using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiGateway.Features.Security;

public class SecurityHeadersMiddleware
{
    private readonly SecurityConfiguration _configuration;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    
    private static readonly Action<ILogger, string, Exception?> LogAddingSecurityHeaders =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(3001, nameof(LogAddingSecurityHeaders)), 
            "Adding security headers for request {Path}");

    public SecurityHeadersMiddleware(SecurityConfiguration configuration, ILogger<SecurityHeadersMiddleware> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public SecurityHeadersMiddleware(SecurityConfiguration configuration)
        : this(configuration, new NullLogger<SecurityHeadersMiddleware>())
    {
    }

    public async Task ProcessRequestAsync(HttpContext context, RequestDelegate next)
    {
        LogAddingSecurityHeaders(_logger, context.Request.Path, null);
        
        if (_configuration.EnableSecurityHeaders)
        {
            AddSecurityHeaders(context);
        }
        
        await next(context).ConfigureAwait(false);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;
        
        headers["X-Frame-Options"] = _configuration.FrameOptions;
        headers["X-Content-Type-Options"] = _configuration.ContentTypeOptions;
        headers["Referrer-Policy"] = _configuration.ReferrerPolicy;
        headers["Content-Security-Policy"] = _configuration.ContentSecurityPolicy;
        
        if (context.Request.IsHttps)
        {
            var hstsValue = $"max-age={_configuration.HstsMaxAge}";
            if (_configuration.EnableHstsIncludeSubDomains)
            {
                hstsValue += "; includeSubDomains";
            }
            if (_configuration.EnableHstsPreload)
            {
                hstsValue += "; preload";
            }
            headers["Strict-Transport-Security"] = hstsValue;
        }
        
        headers.Remove("X-Powered-By");
        headers.Remove("Server");
    }
}
