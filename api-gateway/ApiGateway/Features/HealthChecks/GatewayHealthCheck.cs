namespace ApiGateway.Features.HealthChecks;

public class GatewayHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync()
    {
        var startTime = DateTime.UtcNow;
        var result = new HealthCheckResult
        {
            IsHealthy = true,
            Component = "Gateway",
            Description = "API Gateway is operational",
            CheckedAt = startTime,
            ResponseTime = TimeSpan.FromMilliseconds(1)
        };
        
        return Task.FromResult(result);
    }
}
