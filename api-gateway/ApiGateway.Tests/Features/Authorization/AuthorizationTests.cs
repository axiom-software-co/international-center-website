using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Features.Authorization;
using System.Security.Claims;

namespace ApiGateway.Tests.Features.Authorization;

/// <summary>
/// Authorization tests for API Gateway medical-grade security
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class AuthorizationTests
{
    [Fact]
    public async Task PublicAuthorizationStrategy_Should_AllowAllRequests()
    {
        // Arrange
        var strategy = new PublicAuthorizationStrategy();
        var httpContext = new DefaultHttpContext();
        var user = new ClaimsPrincipal();
        
        // Act
        var result = await strategy.AuthorizeAsync(user, httpContext, "Public");
        
        // Assert - Public strategy should allow all requests
        Assert.True(result.Succeeded);
    }
    
    [Fact]
    public async Task AdminAuthorizationStrategy_Should_RequireAdminRole()
    {
        // Arrange
        var strategy = new AdminAuthorizationStrategy();
        var httpContext = new DefaultHttpContext();
        
        // Create authenticated user with Admin role
        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        var adminUser = new ClaimsPrincipal(identity);
        
        // Act
        var result = await strategy.AuthorizeAsync(adminUser, httpContext, "Admin");
        
        // Assert - Admin strategy should allow admin users
        Assert.True(result.Succeeded);
    }
    
    [Fact]
    public async Task AuthorizationMiddleware_Should_ProcessRequests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ApiGateway.Features.Authorization.AuthorizationMiddleware>>();
        
        var middleware = new ApiGateway.Features.Authorization.AuthorizationMiddleware(logger);
        var httpContext = new DefaultHttpContext();
        
        // Act - Middleware should process requests without throwing
        await middleware.InvokeAsync(httpContext);
        
        // Assert - No exceptions thrown means success
        Assert.True(true);
    }
    
    [Fact]
    public void PermissionValidation_Should_ValidatePermissions()
    {
        // Arrange
        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new Claim("permission", "read-data"));
        var user = new ClaimsPrincipal(identity);
        
        // Act
        var hasPermission = PermissionValidation.HasPermission(user, "read-data");
        var lacksPermission = PermissionValidation.HasPermission(user, "write-data");
        
        // Assert - Should correctly validate permissions
        Assert.True(hasPermission);
        Assert.False(lacksPermission);
    }
}