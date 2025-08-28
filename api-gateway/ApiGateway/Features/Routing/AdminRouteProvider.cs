using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.Features.Routing;

public class AdminRouteProvider
{
    public IEnumerable<RouteConfig> AdminRoutes => new[]
        {
            new RouteConfig
            {
                RouteId = "admin-services",
                ClusterId = "admin-services-cluster",
                Match = new RouteMatch { Path = "/admin/services/{**catch-all}" },
                AuthorizationPolicy = "AdminPolicy"
            }
        };
}
