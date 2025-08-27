namespace ApiGateway.Shared.Configuration;

public class AdminGatewayOptions
{
    public const string SectionName = "AdminGateway";
    
    public bool RequireAuthentication { get; set; } = true;
    public string[] RequiredRoles { get; set; } = Array.Empty<string>();
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string PathPrefix { get; set; } = "/admin";
    public bool EnableSwagger { get; set; } = true;
    public RateLimitOptions RateLimit { get; set; } = new();
}

public class RateLimitOptions
{
    public int RequestsPerMinute { get; set; } = 100;
    public int RequestsPerHour { get; set; } = 1000;
    public bool EnableUserBasedLimiting { get; set; } = true;
}