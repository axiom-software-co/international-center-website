using Xunit;
using ApiGateway.Features.Routing;
using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.Tests.Features.Routing;

/// <summary>
/// Unit tests for API Gateway YARP routing components
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class RoutingUnitTests
{
    [Fact]
    public void RouteConfiguration_Should_HaveDefaultValues()
    {
        // Arrange & Act
        var config = new RouteConfiguration();
        
        // Assert - Configuration should have sensible defaults
        Assert.Equal(TimeSpan.FromSeconds(30), config.DefaultTimeout);
        Assert.True(config.EnableLoadBalancing);
        Assert.Equal("RoundRobin", config.LoadBalancingPolicy);
        Assert.NotEmpty(config.UpstreamClusters);
    }
    
    [Fact]
    public async Task YarpRoutingService_Should_GetRoutes()
    {
        // Arrange
        var config = new RouteConfiguration();
        var publicProvider = new PublicRouteProvider();
        var adminProvider = new AdminRouteProvider(); 
        var routingService = new YarpRoutingService(config, publicProvider, adminProvider);
        
        // Act
        var routes = await routingService.GetRoutesAsync();
        
        // Assert - Should return configured routes
        Assert.NotNull(routes);
        Assert.True(routes.Length > 0);
        Assert.All(routes, route => Assert.NotNull(route.RouteId));
    }
    
    [Fact]
    public async Task YarpRoutingService_Should_GetClusters()
    {
        // Arrange
        var config = new RouteConfiguration();
        var publicProvider = new PublicRouteProvider();
        var adminProvider = new AdminRouteProvider();
        var routingService = new YarpRoutingService(config, publicProvider, adminProvider);
        
        // Act
        var clusters = await routingService.GetClustersAsync();
        
        // Assert - Should return configured clusters
        Assert.NotNull(clusters);
        Assert.True(clusters.Length > 0);
        Assert.All(clusters, cluster => Assert.NotNull(cluster.ClusterId));
    }
    
    [Fact]
    public void PublicRouteProvider_Should_ProvidePublicRoutes()
    {
        // Arrange & Act
        var provider = new PublicRouteProvider();
        var routes = provider.PublicRoutes;
        
        // Assert - Should provide public API routes
        Assert.NotNull(routes);
        Assert.Contains(routes, r => r.Match?.Path?.Contains("/api/services") == true);
        Assert.All(routes, route => Assert.Contains("public", route.ClusterId!.ToLowerInvariant()));
    }
    
    [Fact]
    public void AdminRouteProvider_Should_ProvideAdminRoutes()
    {
        // Arrange & Act
        var provider = new AdminRouteProvider();
        var routes = provider.AdminRoutes;
        
        // Assert - Should provide admin API routes with authorization
        Assert.NotNull(routes);
        Assert.Contains(routes, r => r.Match?.Path?.Contains("/admin/services") == true);
        Assert.All(routes, route => 
        {
            Assert.Contains("admin", route.ClusterId!.ToLowerInvariant());
            Assert.NotNull(route.AuthorizationPolicy);
        });
    }
    
    [Fact]
    public async Task YarpRoutingService_Should_FindRoute()
    {
        // Arrange
        var config = new RouteConfiguration();
        var publicProvider = new PublicRouteProvider();
        var adminProvider = new AdminRouteProvider();
        var routingService = new YarpRoutingService(config, publicProvider, adminProvider);
        
        // Act
        var route = await routingService.FindRouteAsync("/api/services", "GET");
        
        // Assert - Should find matching route
        Assert.NotNull(route);
        Assert.Equal("/api/services", route.Path);
        Assert.Equal("GET", route.Method);
    }
    
    [Fact]
    public void RouteTransformation_Should_TransformPaths()
    {
        // Arrange
        var transformation = new RouteTransformation();
        var originalPath = "/api/services/{id}";
        
        // Act
        var transformedPath = transformation.TransformPath(originalPath);
        
        // Assert - Should apply path transformations
        Assert.NotNull(transformedPath);
        Assert.Contains("services", transformedPath);
    }
    
    [Fact]
    public void LoadBalancing_Should_SelectDestination()
    {
        // Arrange
        var loadBalancer = new LoadBalancing();
        var destinations = new[] { "server1", "server2", "server3" };
        
        // Act
        var selected = loadBalancer.SelectDestination(destinations);
        
        // Assert - Should select a destination
        Assert.NotNull(selected);
        Assert.Contains(selected, destinations);
    }
    
    [Fact]
    public async Task YarpRoutingService_Should_ReloadConfiguration()
    {
        // Arrange
        var config = new RouteConfiguration();
        var publicProvider = new PublicRouteProvider();
        var adminProvider = new AdminRouteProvider();
        var routingService = new YarpRoutingService(config, publicProvider, adminProvider);
        
        // Act & Assert - Should reload without errors
        await routingService.ReloadConfigurationAsync();
        
        // Verify routes are still available after reload
        var routes = await routingService.GetRoutesAsync();
        Assert.NotNull(routes);
        Assert.True(routes.Length > 0);
    }
}