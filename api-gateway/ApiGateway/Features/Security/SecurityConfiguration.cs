namespace ApiGateway.Features.Security;

public class SecurityConfiguration
{
    public bool EnableSecurityHeaders { get; set; } = true;
    public bool EnableRequestValidation { get; set; } = true;
    public bool EnableAntiFraudProtection { get; set; } = true;
    
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
    public string FrameOptions { get; set; } = "DENY";
    public string ContentTypeOptions { get; set; } = "nosniff";
    
    public int HstsMaxAge { get; set; } = 31536000; // 1 year in seconds
    public bool EnableHstsPreload { get; set; } = true;
    public bool EnableHstsIncludeSubDomains { get; set; } = true;
    
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRequestBodySize { get; set; } = 10 * 1024 * 1024; // 10MB
    
    public string[] BlockedUserAgents { get; set; } = [];
    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "DELETE", "OPTIONS", "HEAD"];
    
    public bool ValidateContentSecurityPolicy()
    {
        return !string.IsNullOrWhiteSpace(ContentSecurityPolicy) && 
               ContentSecurityPolicy.Contains("default-src");
    }
}
