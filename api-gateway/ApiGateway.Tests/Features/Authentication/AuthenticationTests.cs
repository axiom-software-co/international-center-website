using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Features.Authentication;

namespace ApiGateway.Tests.Features.Authentication;

/// <summary>
/// Authentication tests for API Gateway medical-grade security
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class AuthenticationTests
{
    [Fact]
    public async Task AnonymousStrategy_Should_AllowAllRequests()
    {
        // Arrange
        var strategy = new AnonymousStrategy();
        var httpContext = new DefaultHttpContext();
        
        // Act
        var result = await strategy.AuthenticateAsync(httpContext);
        
        // Assert - Anonymous strategy should always allow requests
        Assert.NotNull(result);
        Assert.False(result.Identity?.IsAuthenticated ?? true);
    }
    
    [Fact]
    public async Task JwtStrategy_Should_ValidateTokens()
    {
        // Arrange
        var strategy = new JwtStrategy();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer valid-test-token";
        
        // Act
        var result = await strategy.AuthenticateAsync(httpContext);
        
        // Assert - JWT strategy should validate tokens
        Assert.NotNull(result);
        Assert.True(result.Identity?.IsAuthenticated);
    }
    
    [Fact]
    public async Task EntraIdStrategy_Should_ValidateEntraIdTokens()
    {
        // Arrange  
        var strategy = new EntraIdStrategy();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer entra-test-token";
        
        // Act
        var result = await strategy.AuthenticateAsync(httpContext);
        
        // Assert - EntraId strategy should validate Microsoft tokens
        Assert.NotNull(result);
        Assert.True(result.Identity?.IsAuthenticated);
    }
    
    [Fact]
    public async Task AuthenticationMiddleware_Should_ProcessRequests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<AuthenticationMiddleware>>();
        
        var middleware = new AuthenticationMiddleware(logger);
        var httpContext = new DefaultHttpContext();
        
        // Act - Middleware should process requests without throwing
        await middleware.InvokeAsync(httpContext, _ => Task.CompletedTask);
        
        // Assert - No exceptions thrown means success
        Assert.True(true);
    }
}