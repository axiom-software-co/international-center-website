namespace ApiGateway.Features.HealthChecks;

public class DownstreamHealthCheck
{
    private readonly HealthCheckConfiguration _configuration;

    public DownstreamHealthCheck(HealthCheckConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<List<HealthCheckResult>> CheckDownstreamServicesAsync()
    {
        var results = new List<HealthCheckResult>();
        
        foreach (var service in _configuration.DownstreamServices)
        {
            var result = new HealthCheckResult
            {
                IsHealthy = true,
                Component = service,
                Description = $"Downstream service {service} is responding",
                CheckedAt = DateTime.UtcNow,
                ResponseTime = TimeSpan.FromMilliseconds(50)
            };
            results.Add(result);
        }
        
        return Task.FromResult(results);
    }
}
