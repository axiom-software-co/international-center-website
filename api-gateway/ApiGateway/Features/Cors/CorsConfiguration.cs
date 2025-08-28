namespace ApiGateway.Features.Cors;

public class CorsConfiguration
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
    public string[] AllowedHeaders { get; set; } = { "Content-Type", "Authorization", "X-Requested-With" };
    public bool AllowCredentials { get; set; } = true;
    public int MaxAge { get; set; } = 3600; // 1 hour
    public string PolicyName { get; set; } = "Default";
}
