using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Features.ErrorHandling;
using System.Text.Json;

namespace ApiGateway.Tests.Features.ErrorHandling;

/// <summary>
/// Error handling tests for API Gateway medical-grade error management
/// GREEN PHASE: Complete implementation with minimal functionality to make tests pass
/// </summary>
public sealed class ErrorHandlingTests
{
    [Fact]
    public void ErrorConfiguration_Should_HaveDefaultValues()
    {
        // Arrange & Act
        var config = new ErrorConfiguration();
        
        // Assert - Configuration should have sensible defaults
        Assert.False(config.IncludeStackTrace);
        Assert.False(config.IncludeDetails);
        Assert.Contains("Development", config.AllowedDetailEnvironments);
        Assert.Equal(3, config.MaxRetryAttempts);
        Assert.Equal(TimeSpan.FromSeconds(1), config.RetryDelay);
        Assert.True(config.ExceptionStatusCodes.ContainsKey(typeof(ArgumentException)));
    }
    
    [Fact]
    public void ErrorResponseFormatter_Should_FormatExceptions()
    {
        // Arrange
        var formatter = new ErrorResponseFormatter();
        var exception = new ArgumentException("Test error");
        
        // Act
        var result = formatter.FormatError(exception, "test-correlation-id");
        
        // Assert - Should format error as JSON
        Assert.NotNull(result);
        Assert.Contains("Invalid request parameters", result);
        Assert.Contains("test-correlation-id", result);
        Assert.Contains("ArgumentException", result);
        
        var jsonDoc = JsonDocument.Parse(result);
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out _));
    }
    
    [Fact]
    public void ErrorResponseFormatter_Should_MapExceptionStatusCodes()
    {
        // Arrange
        var formatter = new ErrorResponseFormatter();
        
        // Act & Assert - Should map known exceptions to correct status codes
        Assert.Equal(400, formatter.GetStatusCodeForException(new ArgumentException()));
        Assert.Equal(401, formatter.GetStatusCodeForException(new UnauthorizedAccessException()));
        Assert.Equal(501, formatter.GetStatusCodeForException(new NotImplementedException()));
        Assert.Equal(500, formatter.GetStatusCodeForException(new DivideByZeroException())); // Unknown exception type
    }
    
    [Fact]
    public async Task GatewayErrorHandler_Should_HandleErrors()
    {
        // Arrange
        var formatter = new ErrorResponseFormatter();
        var handler = new GatewayErrorHandler(formatter);
        var httpContext = new DefaultHttpContext();
        var exception = new ArgumentException("Test error");
        
        // Act
        await handler.HandleErrorAsync(httpContext, exception);
        
        // Assert - Should set response properties correctly
        Assert.Equal(400, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        Assert.True(httpContext.Response.Headers.ContainsKey("X-Correlation-ID"));
    }
    
    [Fact]
    public void GatewayErrorHandler_Should_DetermineRetryEligibility()
    {
        // Arrange
        var formatter = new ErrorResponseFormatter();
        var handler = new GatewayErrorHandler(formatter);
        
        // Act & Assert - Should correctly determine retry eligibility
        Assert.True(handler.ShouldRetry(new TimeoutException(), 1, 3));
        Assert.True(handler.ShouldRetry(new HttpRequestException(), 1, 3));
        Assert.False(handler.ShouldRetry(new ArgumentException(), 1, 3)); // Not retriable
        Assert.False(handler.ShouldRetry(new TimeoutException(), 3, 3)); // Max attempts reached
    }
    
    [Fact]
    public async Task ErrorHandlingMiddleware_Should_ProcessRequests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var logger = serviceProvider.GetRequiredService<ILogger<ErrorHandlingMiddleware>>();
        var formatter = new ErrorResponseFormatter();
        var errorHandler = new GatewayErrorHandler(formatter);
        
        var middleware = new ErrorHandlingMiddleware(logger, errorHandler);
        var httpContext = new DefaultHttpContext();
        
        // Act - Middleware should process requests without throwing
        await middleware.InvokeAsync(httpContext);
        
        // Assert - No exceptions thrown means success
        Assert.True(true);
    }
    
    [Fact]
    public void ErrorResponseFormatter_Should_IncludeDetailsInDevelopment()
    {
        // Arrange
        var config = new ErrorConfiguration
        {
            IncludeDetails = true,
            IncludeStackTrace = true
        };
        var formatter = new ErrorResponseFormatter(config);
        var exception = new ArgumentException("Detailed error message");
        
        // Act
        var result = formatter.FormatError(exception, "test-id", "Development");
        
        // Assert - Should include details in development environment
        Assert.Contains("Detailed error message", result);
    }
    
    [Fact]
    public void ErrorResponseFormatter_Should_ExcludeDetailsInProduction()
    {
        // Arrange
        var config = new ErrorConfiguration
        {
            IncludeDetails = true,
            IncludeStackTrace = true
        };
        var formatter = new ErrorResponseFormatter(config);
        var exception = new ArgumentException("Sensitive error details");
        
        // Act
        var result = formatter.FormatError(exception, "test-id", "Production");
        
        // Assert - Should exclude details in production environment
        Assert.DoesNotContain("Sensitive error details", result);
        Assert.Contains("Invalid request parameters", result); // Generic message
    }
}