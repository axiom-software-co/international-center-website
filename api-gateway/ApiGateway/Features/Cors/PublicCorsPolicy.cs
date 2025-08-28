namespace ApiGateway.Features.Cors;

public class PublicCorsPolicy : ICorsPolicy
{
    public string Name => "Public";
    
    public CorsConfiguration GetConfiguration()
    {
        // GREEN PHASE: Minimal implementation for public website CORS
        return new CorsConfiguration
        {
            PolicyName = "Public",
            AllowedOrigins = new[] 
            { 
                "http://localhost:3000", 
                "http://localhost:4321", 
                "https://international-center.org" 
            },
            AllowedMethods = new[] { "GET", "POST", "OPTIONS" },
            AllowedHeaders = new[] { "Content-Type", "Authorization", "X-Requested-With" },
            AllowCredentials = false, // Public endpoints don't need credentials
            MaxAge = 3600
        };
    }
    
    public bool IsOriginAllowed(string origin)
    {
        if (string.IsNullOrEmpty(origin))
            return false;
            
        var config = GetConfiguration();
        return config.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
