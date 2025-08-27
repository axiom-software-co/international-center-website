using Yarp.ReverseProxy.Configuration;
using ApiGateway.Shared.Models;

namespace ApiGateway.Features.Routing;

public class YarpRoutingService : IRoutingService
{
    public Task<RouteConfig[]> GetRoutesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<ClusterConfig[]> GetClustersAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task ReloadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<RouteDefinition?> FindRouteAsync(string path, string method)
    {
        throw new NotImplementedException();
    }
}