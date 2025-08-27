namespace ApiGateway.Shared.Configuration;

public class PublicGatewayOptions
{
    public const string SectionName = "PublicGateway";
    
    public bool AllowAnonymous { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string PathPrefix { get; set; } = "/api";
    public bool EnableRateLimiting { get; set; } = true;
    public IpRateLimitOptions RateLimit { get; set; } = new();
}

public class IpRateLimitOptions
{
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
    public string[] WhitelistedIps { get; set; } = Array.Empty<string>();
}