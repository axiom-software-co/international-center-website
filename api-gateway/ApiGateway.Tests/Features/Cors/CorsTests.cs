using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Features.Cors;

namespace ApiGateway.Tests.Features.Cors;

/// <summary>
/// CORS tests for API Gateway medical-grade cross-origin security
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class CorsTests
{
    [Fact]
    public void PublicCorsPolicy_Should_AllowPublicOrigins()
    {
        // Arrange
        var policy = new PublicCorsPolicy();
        
        // Act & Assert - Public policy should allow known public origins
        Assert.True(policy.IsOriginAllowed("http://localhost:4321"));
        Assert.True(policy.IsOriginAllowed("https://international-center.org"));
        Assert.False(policy.IsOriginAllowed("https://malicious-site.com"));
        Assert.False(policy.IsOriginAllowed(""));
    }
    
    [Fact]
    public void AdminCorsPolicy_Should_AllowAdminOrigins()
    {
        // Arrange
        var policy = new AdminCorsPolicy();
        
        // Act & Assert - Admin policy should allow known admin origins
        Assert.True(policy.IsOriginAllowed("http://localhost:3001"));
        Assert.True(policy.IsOriginAllowed("https://admin.international-center.org"));
        Assert.False(policy.IsOriginAllowed("http://localhost:4321"));
        Assert.False(policy.IsOriginAllowed(""));
    }
    
    [Fact]
    public void CorsService_Should_AllowValidOrigins()
    {
        // Arrange
        var corsService = new CorsService();
        
        // Act & Assert - Service should check both public and admin origins
        Assert.True(corsService.IsOriginAllowed("http://localhost:4321")); // Public
        Assert.True(corsService.IsOriginAllowed("http://localhost:3001")); // Admin
        Assert.False(corsService.IsOriginAllowed("https://malicious-site.com"));
    }
    
    [Fact]
    public async Task CorsService_Should_SetCorsHeaders()
    {
        // Arrange
        var corsService = new CorsService();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "http://localhost:4321";
        httpContext.Request.Path = "/api/services";
        
        // Act
        await corsService.ApplyCorsAsync(httpContext);
        
        // Assert - Should set appropriate CORS headers
        Assert.Equal("http://localhost:4321", httpContext.Response.Headers.AccessControlAllowOrigin);
        Assert.Equal("false", httpContext.Response.Headers.AccessControlAllowCredentials); // Public = no credentials
    }
    
    [Fact]
    public async Task CorsMiddleware_Should_ProcessRequests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICorsService, CorsService>();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<CorsMiddleware>>();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        
        var middleware = new CorsMiddleware(logger, corsService);
        var httpContext = new DefaultHttpContext();
        
        // Act - Middleware should process requests without throwing
        await middleware.InvokeAsync(httpContext);
        
        // Assert - No exceptions thrown means success
        Assert.True(true);
    }
    
    [Fact]
    public async Task CorsMiddleware_Should_HandlePreflightRequests()
    {
        // Arrange
        var corsService = new CorsService();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "OPTIONS";
        httpContext.Request.Headers.Origin = "http://localhost:4321";
        httpContext.Request.Path = "/api/services";
        
        // Act
        await corsService.ApplyCorsAsync(httpContext);
        
        // Assert - Should handle preflight with 204 status
        Assert.Equal(204, httpContext.Response.StatusCode);
        Assert.Contains("GET", httpContext.Response.Headers.AccessControlAllowMethods.ToString());
    }
}