namespace ApiGateway.Features.Cors;

public class AdminCorsPolicy : ICorsPolicy
{
    public string Name => "Admin";
    
    public CorsConfiguration GetConfiguration()
    {
        // GREEN PHASE: Minimal implementation for admin dashboard CORS
        return new CorsConfiguration
        {
            PolicyName = "Admin",
            AllowedOrigins = new[] 
            { 
                "http://localhost:3001", 
                "https://admin.international-center.org" 
            },
            AllowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" },
            AllowedHeaders = new[] { "Content-Type", "Authorization", "X-Requested-With", "X-Admin-Token" },
            AllowCredentials = true, // Admin endpoints require authentication
            MaxAge = 1800 // 30 minutes (shorter for admin security)
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
