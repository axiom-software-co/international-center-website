namespace ApiGateway.Features.Authentication;

public class AuthenticationConfiguration
{
    public const string SectionName = "Authentication";
    
    public bool EnableAuthentication { get; set; } = true;
    public string DefaultScheme { get; set; } = "JWT";
    public string[] SupportedSchemes { get; set; } = Array.Empty<string>();
    public JwtAuthenticationOptions Jwt { get; set; } = new();
    public EntraIdAuthenticationOptions EntraId { get; set; } = new();
}

public class JwtAuthenticationOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(1);
}

public class EntraIdAuthenticationOptions
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}