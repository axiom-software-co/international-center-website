using System.Net;

namespace ApiGateway.Features.RateLimiting;

public class RateLimitConfiguration
{
    public int DefaultRequestsPerMinute { get; set; } = 1000;
    public int AdminRequestsPerMinute { get; set; } = 100;
    public TimeSpan WindowSize { get; set; } = TimeSpan.FromMinutes(1);
    public bool EnableIpBasedLimiting { get; set; } = true;
    public bool EnableUserBasedLimiting { get; set; } = true;
    public HashSet<string> BypassIpAddresses { get; set; } = new() { "127.0.0.1", "::1", "localhost" };

    public bool ShouldBypassRateLimit(IPAddress ipAddress)
    {
        var ipString = ipAddress.ToString();
        return BypassIpAddresses.Contains(ipString);
    }
}
