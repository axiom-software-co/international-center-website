using Microsoft.AspNetCore.Http;

namespace ApiGateway.Shared.Models;

public class GatewayResponse
{
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    
    public static GatewayResponse FromHttpContext(HttpContext context)
    {
        throw new NotImplementedException();
    }
}