namespace ApiGateway.Features.HealthChecks;

public class HealthCheckConfiguration
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    public bool IncludeDetails { get; set; } = true;
    public string[] DownstreamServices { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> Tags { get; set; } = new();
    public string HealthCheckPath { get; set; } = "/health";
    public string ReadyPath { get; set; } = "/ready";
    public string LivePath { get; set; } = "/live";
}
