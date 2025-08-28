using Yarp.ReverseProxy.Configuration;
using ApiGateway.Shared.Models;

namespace ApiGateway.Features.Routing;

public class PublicRouteProvider
{
    public IEnumerable<RouteConfig> PublicRoutes => new[]
        {
            new RouteConfig
            {
                RouteId = "public-services",
                ClusterId = "public-services-cluster",
                Match = new RouteMatch { Path = "/api/services/{**catch-all}" }
            }
        };
}
