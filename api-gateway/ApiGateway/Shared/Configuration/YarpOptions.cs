namespace ApiGateway.Shared.Configuration;

public class YarpOptions
{
    public const string SectionName = "Yarp";
    
    public string ConfigurationSource { get; set; } = "appsettings";
    public TimeSpan ConfigurationRefreshInterval { get; set; } = TimeSpan.FromMinutes(5);
    public LoadBalancingOptions LoadBalancing { get; set; } = new();
    public HealthCheckOptions HealthChecks { get; set; } = new();
    public Dictionary<string, ClusterOptions> Clusters { get; set; } = new();
}

public class LoadBalancingOptions
{
    public string Policy { get; set; } = "RoundRobin";
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(30);
}

public class HealthCheckOptions
{
    public bool Enabled { get; set; } = true;
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

public class ClusterOptions
{
    public string[] Destinations { get; set; } = Array.Empty<string>();
    public string LoadBalancingPolicy { get; set; } = string.Empty;
}