namespace ApiGateway.Features.RateLimiting;

public static class RateLimitPolicies
{
    public static RateLimitPolicy GetPublicPolicy()
    {
        return new RateLimitPolicy
        {
            RequestsPerMinute = 1000,
            WindowSize = TimeSpan.FromMinutes(1)
        };
    }

    public static RateLimitPolicy GetAdminPolicy()
    {
        return new RateLimitPolicy
        {
            RequestsPerMinute = 100,
            WindowSize = TimeSpan.FromMinutes(1)
        };
    }
}

public class RateLimitPolicy
{
    public int RequestsPerMinute { get; set; }
    public TimeSpan WindowSize { get; set; }
}
