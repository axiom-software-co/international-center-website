using Yarp.ReverseProxy.Configuration;
using ApiGateway.Shared.Models;

namespace ApiGateway.Features.Routing;

public class YarpRoutingService : IRoutingService
{
    private readonly RouteConfiguration _configuration;
    private readonly PublicRouteProvider _publicProvider;
    private readonly AdminRouteProvider _adminProvider;

    public YarpRoutingService(
        RouteConfiguration configuration,
        PublicRouteProvider publicProvider,
        AdminRouteProvider adminProvider)
    {
        _configuration = configuration;
        _publicProvider = publicProvider;
        _adminProvider = adminProvider;
    }

    public Task<RouteConfig[]> GetRoutesAsync(CancellationToken cancellationToken = default)
    {
        var publicRoutes = _publicProvider.PublicRoutes;
        var adminRoutes = _adminProvider.AdminRoutes;
        var allRoutes = publicRoutes.Concat(adminRoutes).ToArray();
        return Task.FromResult(allRoutes);
    }

    public Task<ClusterConfig[]> GetClustersAsync(CancellationToken cancellationToken = default)
    {
        var clusters = new[]
        {
            new ClusterConfig
            {
                ClusterId = "public-services-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "destination1", new DestinationConfig { Address = "https://localhost:5001" } }
                }
            },
            new ClusterConfig
            {
                ClusterId = "admin-services-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "destination1", new DestinationConfig { Address = "https://localhost:5002" } }
                }
            }
        };
        return Task.FromResult(clusters);
    }

    public Task ReloadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<RouteDefinition?> FindRouteAsync(string path, string method)
    {
        var route = new RouteDefinition
        {
            RouteId = "matched-route",
            Path = path,
            Method = method,
            ClusterId = path.StartsWith("/admin") ? "admin-services-cluster" : "public-services-cluster"
        };
        return Task.FromResult<RouteDefinition?>(route);
    }
}
