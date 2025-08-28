using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Features.HealthChecks;
using System.Text.Json;

namespace ApiGateway.Tests.Features.HealthChecks;

/// <summary>
/// Health check tests for API Gateway medical-grade monitoring infrastructure
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class HealthCheckTests
{
    [Fact]
    public void HealthCheckConfiguration_Should_HaveDefaultValues()
    {
        // Arrange & Act
        var config = new HealthCheckConfiguration();
        
        // Assert - Configuration should have sensible defaults for medical-grade monitoring
        Assert.Equal(TimeSpan.FromSeconds(10), config.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(30), config.CheckInterval);
        Assert.Equal(3, config.RetryCount);
        Assert.Equal(TimeSpan.FromSeconds(2), config.RetryDelay);
        Assert.True(config.IncludeDetails);
        Assert.Empty(config.DownstreamServices);
        Assert.Empty(config.Tags);
        Assert.Equal("/health", config.HealthCheckPath);
        Assert.Equal("/ready", config.ReadyPath);
        Assert.Equal("/live", config.LivePath);
    }
    
    [Fact]
    public async Task GatewayHealthCheck_Should_CheckGatewayStatus()
    {
        // Arrange
        var healthCheck = new GatewayHealthCheck();
        
        // Act
        var result = await healthCheck.CheckHealthAsync();
        
        // Assert - Should return health check result
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
        Assert.Equal("Gateway", result.Component);
        Assert.Contains("operational", result.Description);
    }
    
    [Fact]
    public async Task DownstreamHealthCheck_Should_CheckDownstreamServices()
    {
        // Arrange
        var config = new HealthCheckConfiguration
        {
            DownstreamServices = new[] { "service1", "service2" }
        };
        var healthCheck = new DownstreamHealthCheck(config);
        
        // Act
        var results = await healthCheck.CheckDownstreamServicesAsync();
        
        // Assert - Should check configured downstream services
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.NotNull(r.Component));
    }
    
    [Fact]
    public void HealthCheckResult_Should_SerializeToJson()
    {
        // Arrange
        var result = new HealthCheckResult
        {
            IsHealthy = true,
            Component = "TestComponent",
            Description = "Test description",
            CheckedAt = DateTime.UtcNow,
            ResponseTime = TimeSpan.FromMilliseconds(100)
        };
        
        // Act
        var json = JsonSerializer.Serialize(result);
        var deserialized = JsonSerializer.Deserialize<HealthCheckResult>(json);
        
        // Assert - Should serialize and deserialize correctly
        Assert.NotNull(deserialized);
        Assert.Equal(result.IsHealthy, deserialized.IsHealthy);
        Assert.Equal(result.Component, deserialized.Component);
        Assert.Equal(result.Description, deserialized.Description);
    }
    
    [Fact]
    public async Task HealthCheckService_Should_PerformHealthChecks()
    {
        // Arrange
        var config = new HealthCheckConfiguration();
        var gatewayHealthCheck = new GatewayHealthCheck();
        var downstreamHealthCheck = new DownstreamHealthCheck(config);
        var service = new HealthCheckService(config, gatewayHealthCheck, downstreamHealthCheck);
        
        // Act
        var result = await service.PerformHealthCheckAsync();
        
        // Assert - Should perform comprehensive health check
        Assert.NotNull(result);
        Assert.NotNull(result.OverallStatus);
        Assert.NotNull(result.Components);
        Assert.True(result.Components.Count > 0);
    }
    
    [Fact]
    public async Task HealthCheckMiddleware_Should_HandleHealthEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<HealthCheckMiddleware>>();
        var config = new HealthCheckConfiguration();
        var gatewayHealthCheck = new GatewayHealthCheck();
        var downstreamHealthCheck = new DownstreamHealthCheck(config);
        var healthService = new HealthCheckService(config, gatewayHealthCheck, downstreamHealthCheck);
        
        var middleware = new HealthCheckMiddleware(logger, healthService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/health";
        httpContext.Request.Method = "GET";
        
        // Act
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should handle health check endpoint
        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
    }
    
    [Fact]
    public async Task HealthCheckMiddleware_Should_HandleReadyEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<HealthCheckMiddleware>>();
        var config = new HealthCheckConfiguration();
        var gatewayHealthCheck = new GatewayHealthCheck();
        var downstreamHealthCheck = new DownstreamHealthCheck(config);
        var healthService = new HealthCheckService(config, gatewayHealthCheck, downstreamHealthCheck);
        
        var middleware = new HealthCheckMiddleware(logger, healthService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/ready";
        httpContext.Request.Method = "GET";
        
        // Act
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should handle readiness endpoint
        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
    }
    
    [Fact]
    public async Task HealthCheckMiddleware_Should_HandleLiveEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<HealthCheckMiddleware>>();
        var config = new HealthCheckConfiguration();
        var gatewayHealthCheck = new GatewayHealthCheck();
        var downstreamHealthCheck = new DownstreamHealthCheck(config);
        var healthService = new HealthCheckService(config, gatewayHealthCheck, downstreamHealthCheck);
        
        var middleware = new HealthCheckMiddleware(logger, healthService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/live";
        httpContext.Request.Method = "GET";
        
        // Act
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should handle liveness endpoint
        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
    }
    
    [Fact]
    public void HealthCheckService_Should_DetermineOverallStatus()
    {
        // Arrange
        var config = new HealthCheckConfiguration();
        var gatewayHealthCheck = new GatewayHealthCheck();
        var downstreamHealthCheck = new DownstreamHealthCheck(config);
        var service = new HealthCheckService(config, gatewayHealthCheck, downstreamHealthCheck);
        
        var healthyResults = new List<HealthCheckResult>
        {
            new() { IsHealthy = true, Component = "Component1" },
            new() { IsHealthy = true, Component = "Component2" }
        };
        
        var mixedResults = new List<HealthCheckResult>
        {
            new() { IsHealthy = true, Component = "Component1" },
            new() { IsHealthy = false, Component = "Component2" }
        };
        
        // Act & Assert - Should determine overall status correctly
        Assert.Equal("Healthy", service.DetermineOverallStatus(healthyResults));
        Assert.Equal("Unhealthy", service.DetermineOverallStatus(mixedResults));
    }
}