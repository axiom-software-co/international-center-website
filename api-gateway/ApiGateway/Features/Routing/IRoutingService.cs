using Yarp.ReverseProxy.Configuration;
using ApiGateway.Shared.Models;

namespace ApiGateway.Features.Routing;

public interface IRoutingService
{
    Task<RouteConfig[]> GetRoutesAsync(CancellationToken cancellationToken = default);
    Task<ClusterConfig[]> GetClustersAsync(CancellationToken cancellationToken = default);
    Task ReloadConfigurationAsync(CancellationToken cancellationToken = default);
    Task<RouteDefinition?> FindRouteAsync(string path, string method);
}