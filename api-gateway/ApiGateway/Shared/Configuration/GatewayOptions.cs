namespace ApiGateway.Shared.Configuration;

public class GatewayOptions
{
    public const string SectionName = "Gateway";
    
    public string Environment { get; set; } = string.Empty;
    public bool EnablePublicApi { get; set; } = true;
    public bool EnableAdminApi { get; set; } = true;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxConcurrentRequests { get; set; } = 1000;
    public string[] TrustedProxies { get; set; } = Array.Empty<string>();
    public bool EnableHttpsRedirection { get; set; } = true;
    public string BaseUrl { get; set; } = string.Empty;
}