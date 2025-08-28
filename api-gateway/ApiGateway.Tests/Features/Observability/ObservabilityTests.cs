using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Features.Observability;
using System.Diagnostics;
using System.Text.Json;

namespace ApiGateway.Tests.Features.Observability;

/// <summary>
/// Observability tests for API Gateway medical-grade monitoring and audit trail compliance
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class ObservabilityTests
{
    [Fact]
    public void ObservabilityConfiguration_Should_HaveDefaultValues()
    {
        // Arrange & Act
        var config = new ObservabilityConfiguration();
        
        // Assert - Configuration should have sensible defaults for medical-grade observability
        Assert.True(config.EnableRequestLogging);
        Assert.True(config.EnableMetricsCollection);
        Assert.True(config.EnableTracing);
        Assert.True(config.IncludeRequestBody);
        Assert.True(config.IncludeResponseBody);
        Assert.Equal(TimeSpan.FromSeconds(30), config.MetricsFlushInterval);
        Assert.Contains("UserId", config.SensitiveFields);
        Assert.Contains("Authorization", config.SensitiveHeaders);
    }
    
    [Fact]
    public void GatewayMetrics_Should_InitializeCounters()
    {
        // Arrange & Act
        var metrics = new GatewayMetrics();
        
        // Assert - Should initialize metric counters
        Assert.NotNull(metrics);
        Assert.Equal(0, metrics.TotalRequests);
        Assert.Equal(0, metrics.SuccessfulRequests);
        Assert.Equal(0, metrics.FailedRequests);
        Assert.Equal(TimeSpan.Zero, metrics.AverageResponseTime);
    }
    
    [Fact]
    public void GatewayMetrics_Should_TrackRequestMetrics()
    {
        // Arrange
        var metrics = new GatewayMetrics();
        
        // Act
        metrics.IncrementTotalRequests();
        metrics.IncrementSuccessfulRequests();
        metrics.RecordResponseTime(TimeSpan.FromMilliseconds(150));
        
        // Assert - Should track request metrics correctly
        Assert.Equal(1, metrics.TotalRequests);
        Assert.Equal(1, metrics.SuccessfulRequests);
        Assert.Equal(0, metrics.FailedRequests);
        Assert.Equal(TimeSpan.FromMilliseconds(150), metrics.AverageResponseTime);
    }
    
    [Fact]
    public async Task RequestLoggingMiddleware_Should_LogRequests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<RequestLoggingMiddleware>>();
        var config = new ObservabilityConfiguration();
        var middleware = new RequestLoggingMiddleware(logger, config);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
        httpContext.Request.Method = "GET";
        httpContext.TraceIdentifier = "test-trace-id";
        
        // Act
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should log request with trace identifier
        Assert.Equal("test-trace-id", httpContext.TraceIdentifier);
        Assert.True(httpContext.Response.Headers.ContainsKey("X-Correlation-ID"));
    }
    
    [Fact]
    public async Task MetricsCollectionMiddleware_Should_CollectMetrics()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<MetricsCollectionMiddleware>>();
        var metrics = new GatewayMetrics();
        var middleware = new MetricsCollectionMiddleware(logger, metrics);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
        httpContext.Request.Method = "GET";
        
        // Act
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should collect request metrics
        Assert.True(metrics.TotalRequests > 0);
        Assert.True(httpContext.Response.Headers.ContainsKey("X-Response-Time"));
    }
    
    [Fact]
    public async Task TracingMiddleware_Should_AddTracing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<TracingMiddleware>>();
        var config = new ObservabilityConfiguration();
        var middleware = new TracingMiddleware(logger, config);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
        httpContext.Request.Method = "GET";
        
        // Act
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should add tracing headers
        Assert.True(httpContext.Response.Headers.ContainsKey("X-Trace-ID"));
        Assert.True(httpContext.Response.Headers.ContainsKey("X-Request-ID"));
    }
    
    [Fact]
    public void ObservabilityConfiguration_Should_RedactSensitiveData()
    {
        // Arrange
        var config = new ObservabilityConfiguration();
        var sensitiveData = new Dictionary<string, object>
        {
            { "UserId", "12345" },
            { "PublicData", "visible" },
            { "Authorization", "Bearer token123" }
        };
        
        // Act
        var redacted = config.RedactSensitiveData(sensitiveData);
        
        // Assert - Should redact sensitive fields
        Assert.Equal("[REDACTED]", redacted["UserId"]);
        Assert.Equal("visible", redacted["PublicData"]);
        Assert.Equal("[REDACTED]", redacted["Authorization"]);
    }
    
    [Fact]
    public void GatewayMetrics_Should_CalculateAverageResponseTime()
    {
        // Arrange
        var metrics = new GatewayMetrics();
        
        // Act
        metrics.RecordResponseTime(TimeSpan.FromMilliseconds(100));
        metrics.RecordResponseTime(TimeSpan.FromMilliseconds(200));
        metrics.RecordResponseTime(TimeSpan.FromMilliseconds(300));
        
        // Assert - Should calculate correct average response time
        Assert.Equal(TimeSpan.FromMilliseconds(200), metrics.AverageResponseTime);
    }
    
    [Fact]
    public void GatewayMetrics_Should_SerializeToJson()
    {
        // Arrange
        var metrics = new GatewayMetrics();
        metrics.IncrementTotalRequests();
        metrics.IncrementSuccessfulRequests();
        metrics.RecordResponseTime(TimeSpan.FromMilliseconds(150));
        
        // Act
        var json = JsonSerializer.Serialize(metrics);
        var deserialized = JsonSerializer.Deserialize<GatewayMetrics>(json);
        
        // Assert - Should serialize and deserialize correctly
        Assert.NotNull(deserialized);
        Assert.Equal(metrics.TotalRequests, deserialized.TotalRequests);
        Assert.Equal(metrics.SuccessfulRequests, deserialized.SuccessfulRequests);
    }
    
    [Fact]
    public void ObservabilityConfiguration_Should_ValidateConfiguration()
    {
        // Arrange
        var config = new ObservabilityConfiguration
        {
            MetricsFlushInterval = TimeSpan.FromSeconds(5)
        };
        
        // Act & Assert - Should validate configuration values
        Assert.True(config.IsValid());
        
        // Invalid configuration
        config.MetricsFlushInterval = TimeSpan.Zero;
        Assert.False(config.IsValid());
    }
}