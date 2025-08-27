using Microsoft.AspNetCore.Http;
using System.Net;

namespace ApiGateway.Shared.Utilities;

public static class RequestHelpers
{
    public static string GetClientIpAddress(this HttpContext context)
    {
        throw new NotImplementedException();
    }
    
    public static string GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty;
    }
    
    public static bool IsHealthCheckRequest(this HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/health");
    }
    
    public static bool IsMetricsRequest(this HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/metrics");
    }
    
    public static string GetCorrelationId(this HttpContext context)
    {
        throw new NotImplementedException();
    }
}