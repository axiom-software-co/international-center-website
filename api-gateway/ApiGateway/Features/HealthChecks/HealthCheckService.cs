namespace ApiGateway.Features.HealthChecks;

public class HealthCheckService
{
    private readonly HealthCheckConfiguration _configuration;
    private readonly GatewayHealthCheck _gatewayHealthCheck;
    private readonly DownstreamHealthCheck _downstreamHealthCheck;

    public HealthCheckService(
        HealthCheckConfiguration configuration,
        GatewayHealthCheck gatewayHealthCheck,
        DownstreamHealthCheck downstreamHealthCheck)
    {
        _configuration = configuration;
        _gatewayHealthCheck = gatewayHealthCheck;
        _downstreamHealthCheck = downstreamHealthCheck;
    }

    public async Task<HealthCheckResponse> PerformHealthCheckAsync()
    {
        var components = new List<HealthCheckResult>();

        var gatewayResult = await _gatewayHealthCheck.CheckHealthAsync().ConfigureAwait(false);
        components.Add(gatewayResult);

        var downstreamResults = await _downstreamHealthCheck.CheckDownstreamServicesAsync().ConfigureAwait(false);
        components.AddRange(downstreamResults);

        var overallStatus = DetermineOverallStatus(components);

        return new HealthCheckResponse
        {
            OverallStatus = overallStatus,
            Components = components,
            CheckedAt = DateTime.UtcNow
        };
    }

    public string DetermineOverallStatus(IEnumerable<HealthCheckResult> results)
    {
        return results.All(r => r.IsHealthy) ? "Healthy" : "Unhealthy";
    }
}

public class HealthCheckResponse
{
    public string OverallStatus { get; set; } = string.Empty;
    public List<HealthCheckResult> Components { get; set; } = new();
    public DateTime CheckedAt { get; set; }
}