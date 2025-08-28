using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Features.RateLimiting;
using System.Net;

namespace ApiGateway.Tests.Features.RateLimiting;

/// <summary>
/// Rate limiting tests for API Gateway medical-grade resource protection and abuse prevention
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class RateLimitingTests
{
    [Fact]
    public void RateLimitConfiguration_Should_HaveDefaultValues()
    {
        // Arrange & Act
        var config = new RateLimitConfiguration();
        
        // Assert - Configuration should have medical-grade defaults
        Assert.Equal(1000, config.DefaultRequestsPerMinute);
        Assert.Equal(100, config.AdminRequestsPerMinute);
        Assert.Equal(TimeSpan.FromMinutes(1), config.WindowSize);
        Assert.True(config.EnableIpBasedLimiting);
        Assert.True(config.EnableUserBasedLimiting);
        Assert.NotEmpty(config.BypassIpAddresses);
        Assert.Contains("127.0.0.1", config.BypassIpAddresses);
    }
    
    [Fact]
    public async Task IpBasedRateLimiter_Should_LimitRequests()
    {
        // Arrange
        var config = new RateLimitConfiguration { DefaultRequestsPerMinute = 5 };
        var rateLimiter = new IpBasedRateLimiter(config, new RedisRateLimitStore());
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
        
        // Act & Assert - Should allow first 5 requests
        for (int i = 0; i < 5; i++)
        {
            var result = await rateLimiter.IsAllowedAsync(httpContext);
            Assert.True(result);
        }
        
        // 6th request should be blocked
        var blockedResult = await rateLimiter.IsAllowedAsync(httpContext);
        Assert.False(blockedResult);
    }
    
    [Fact]
    public async Task UserBasedRateLimiter_Should_LimitRequests()
    {
        // Arrange
        var config = new RateLimitConfiguration { AdminRequestsPerMinute = 3 };
        var rateLimiter = new UserBasedRateLimiter(config, new RedisRateLimitStore());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-ID"] = "admin-user-123";
        
        // Act & Assert - Should allow first 3 requests for admin user
        for (int i = 0; i < 3; i++)
        {
            var result = await rateLimiter.IsAllowedAsync(httpContext);
            Assert.True(result);
        }
        
        // 4th request should be blocked
        var blockedResult = await rateLimiter.IsAllowedAsync(httpContext);
        Assert.False(blockedResult);
    }
    
    [Fact]
    public void RedisRateLimitStore_Should_InitializeCorrectly()
    {
        // Arrange & Act
        var store = new RedisRateLimitStore();
        
        // Assert - Should initialize without errors
        Assert.NotNull(store);
        Assert.True(store.IsConnected);
    }
    
    [Fact]
    public async Task RedisRateLimitStore_Should_TrackRequestCounts()
    {
        // Arrange
        var store = new RedisRateLimitStore();
        var key = "test-key-123";
        var windowSize = TimeSpan.FromMinutes(1);
        
        // Act
        var initialCount = await store.GetRequestCountAsync(key, windowSize);
        await store.IncrementRequestCountAsync(key, windowSize);
        await store.IncrementRequestCountAsync(key, windowSize);
        var finalCount = await store.GetRequestCountAsync(key, windowSize);
        
        // Assert - Should track request counts accurately
        Assert.Equal(0, initialCount);
        Assert.Equal(2, finalCount);
    }
    
    [Fact]
    public async Task RateLimitingService_Should_CoordinateRateLimiters()
    {
        // Arrange
        var config = new RateLimitConfiguration();
        var ipLimiter = new IpBasedRateLimiter(config, new RedisRateLimitStore());
        var userLimiter = new UserBasedRateLimiter(config, new RedisRateLimitStore());
        var service = new RateLimitingService(config, ipLimiter, userLimiter);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        httpContext.Request.Headers["X-User-ID"] = "user-456";
        
        // Act
        var result = await service.IsAllowedAsync(httpContext);
        var info = await service.GetRateLimitInfoAsync(httpContext);
        
        // Assert - Should coordinate both IP and user-based limiting
        Assert.True(result);
        Assert.NotNull(info);
        Assert.True(info.IsAllowed);
        Assert.Equal(1000, info.Limit);
    }
    
    [Fact]
    public async Task RateLimitingMiddleware_Should_EnforceRateLimits()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<RateLimitingMiddleware>>();
        var config = new RateLimitConfiguration();
        var ipLimiter = new IpBasedRateLimiter(config, new RedisRateLimitStore());
        var userLimiter = new UserBasedRateLimiter(config, new RedisRateLimitStore());
        var rateLimitService = new RateLimitingService(config, ipLimiter, userLimiter);
        var middleware = new RateLimitingMiddleware(logger, rateLimitService);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.2");
        
        // Act
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should allow request and add rate limit headers
        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.True(httpContext.Response.Headers.ContainsKey("X-RateLimit-Limit"));
        Assert.True(httpContext.Response.Headers.ContainsKey("X-RateLimit-Remaining"));
    }
    
    [Fact]
    public void RateLimitPolicies_Should_ProvideCorrectPolicies()
    {
        // Arrange & Act
        var publicPolicy = RateLimitPolicies.GetPublicPolicy();
        var adminPolicy = RateLimitPolicies.GetAdminPolicy();
        
        // Assert - Should provide medical-grade rate limit policies
        Assert.Equal(1000, publicPolicy.RequestsPerMinute);
        Assert.Equal(100, adminPolicy.RequestsPerMinute);
        Assert.Equal(TimeSpan.FromMinutes(1), publicPolicy.WindowSize);
        Assert.Equal(TimeSpan.FromMinutes(1), adminPolicy.WindowSize);
    }
    
    [Fact]
    public void RateLimitConfiguration_Should_ValidateBypassAddresses()
    {
        // Arrange
        var config = new RateLimitConfiguration();
        var testIp = IPAddress.Parse("127.0.0.1");
        var publicIp = IPAddress.Parse("8.8.8.8");
        
        // Act & Assert - Should correctly identify bypass addresses
        Assert.True(config.ShouldBypassRateLimit(testIp));
        Assert.False(config.ShouldBypassRateLimit(publicIp));
    }
    
    [Fact]
    public async Task RateLimitingMiddleware_Should_Return429WhenExceeded()
    {
        // Arrange
        var config = new RateLimitConfiguration { DefaultRequestsPerMinute = 1 };
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<RateLimitingMiddleware>>();
        var ipLimiter = new IpBasedRateLimiter(config, new RedisRateLimitStore());
        var userLimiter = new UserBasedRateLimiter(config, new RedisRateLimitStore());
        var rateLimitService = new RateLimitingService(config, ipLimiter, userLimiter);
        var middleware = new RateLimitingMiddleware(logger, rateLimitService);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.2.100");
        
        // Act - First request should pass
        await middleware.InvokeAsync(httpContext);
        Assert.Equal(200, httpContext.Response.StatusCode);
        
        // Second request should be rate limited
        httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.2.100");
        await middleware.InvokeAsync(httpContext);
        
        // Assert - Should return 429 Too Many Requests
        Assert.Equal(429, httpContext.Response.StatusCode);
        Assert.True(httpContext.Response.Headers.ContainsKey("Retry-After"));
    }
}