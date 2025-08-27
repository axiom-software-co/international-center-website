namespace ApiGateway.Shared.Configuration;

public class MiddlewareOptions
{
    public const string SectionName = "Middleware";
    
    public bool EnableAuthentication { get; set; } = true;
    public bool EnableAuthorization { get; set; } = true;
    public bool EnableRateLimiting { get; set; } = true;
    public bool EnableCors { get; set; } = true;
    public bool EnableSecurity { get; set; } = true;
    public bool EnableObservability { get; set; } = true;
    public bool EnableHealthChecks { get; set; } = true;
    public bool EnableErrorHandling { get; set; } = true;
    public Dictionary<string, int> MiddlewarePriority { get; set; } = new();
}