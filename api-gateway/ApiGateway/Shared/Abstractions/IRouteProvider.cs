using Yarp.ReverseProxy.Configuration;
using ApiGateway.Shared.Models;

namespace ApiGateway.Shared.Abstractions;

public interface IRouteProvider
{
    Task<IEnumerable<RouteConfig>> GetRoutesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ClusterConfig>> GetClustersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RouteDefinition>> GetRouteDefinitionsAsync(CancellationToken cancellationToken = default);
    Task ReloadRoutesAsync(CancellationToken cancellationToken = default);
}