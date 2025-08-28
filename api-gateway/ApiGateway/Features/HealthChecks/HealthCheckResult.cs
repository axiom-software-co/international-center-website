namespace ApiGateway.Features.HealthChecks;

public class HealthCheckResult
{
    public bool IsHealthy { get; set; }
    public string Component { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}