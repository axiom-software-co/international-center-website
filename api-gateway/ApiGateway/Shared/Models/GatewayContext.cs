using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGateway.Shared.Models;

public class GatewayContext
{
    public HttpContext HttpContext { get; set; } = null!;
    public ClaimsPrincipal User { get; set; } = new();
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;
    public string ClientIpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public bool IsAuthenticated { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string RouteId { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
}