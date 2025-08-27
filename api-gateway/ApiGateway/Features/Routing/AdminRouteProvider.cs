using Yarp.ReverseProxy.Configuration;
using ApiGateway.Shared.Models;
using ApiGateway.Shared.Abstractions;

namespace ApiGateway.Features.Routing;

public class AdminRouteProvider : IRouteProvider
{
    public Task<IEnumerable<RouteConfig>> GetRoutesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<IEnumerable<ClusterConfig>> GetClustersAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<IEnumerable<RouteDefinition>> GetRouteDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task ReloadRoutesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}