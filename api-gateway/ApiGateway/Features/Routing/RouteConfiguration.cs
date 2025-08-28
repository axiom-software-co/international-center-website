namespace ApiGateway.Features.Routing;

public class RouteConfiguration
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableLoadBalancing { get; set; } = true;
    public string LoadBalancingPolicy { get; set; } = "RoundRobin";
    public Dictionary<string, string> UpstreamClusters { get; set; } = new()
    {
        { "services-domain", "https://localhost:5001" },
        { "admin-services", "https://localhost:5002" }
    };
}
