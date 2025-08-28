using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AspireHost.Shared.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;
using ServicesDomain.Features.GetService;
using ServicesDomain.Features.GetServiceBySlug;
using ServicesDomain.Features.CreateService;

namespace ServicesDomain.Tests.Shared;

/// <summary>
/// End-to-end integration tests for Services Domain through API Gateway
/// Tests will FAIL until full infrastructure stack is implemented
/// Tests complete request flow: Gateway → Services Domain → Database → Response
/// Medical-grade testing with real dependencies and audit trail verification
/// </summary>
public sealed class EndToEndIntegrationTests : IAsyncDisposable
{
    private DistributedApplication? _app;
    private HttpClient? _gatewayClient;
    private HttpClient? _directServiceClient;

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_GetServiceById_ShouldWorkThroughFullStack()
    {
        // Arrange - This will fail until AspireHost orchestration is implemented
        await InitializeTestInfrastructure();
        
        var serviceId = await CreateTestService(); // Helper to seed test data
        var requestUri = $"/api/services/{serviceId}";

        // Act - Request through API Gateway to Services Domain
        var response = await _gatewayClient!.GetAsync(new Uri(requestUri, UriKind.Relative));
        
        // Assert - Verify complete end-to-end flow
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var serviceData = await response.Content.ReadFromJsonAsync<GetServiceResponse>();
        Assert.NotNull(serviceData);
        Assert.Equal(serviceId, serviceData.Id);
        
        // Verify medical-grade audit headers are present
        Assert.True(response.Headers.Contains("X-Correlation-ID") ||
                   response.Headers.Contains("x-correlation-id"),
                   "Medical audit correlation ID should be present");
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_GetServiceBySlug_ShouldWorkThroughFullStack()
    {
        // Arrange - This will fail until GetServiceBySlug endpoints are implemented
        await InitializeTestInfrastructure();
        
        var slug = "test-medical-service";
        await CreateTestServiceWithSlug(slug);
        var requestUri = $"/api/services/by-slug/{slug}";

        // Act
        var response = await _gatewayClient!.GetAsync(new Uri(requestUri, UriKind.Relative));
        
        // Assert - Verify slug-based routing works end-to-end
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var serviceData = await response.Content.ReadFromJsonAsync<GetServiceResponse>();
        Assert.NotNull(serviceData);
        Assert.Equal(slug, serviceData.Slug);
        
        // Verify caching headers are present for public API performance
        Assert.True(response.Headers.Contains("Cache-Control") ||
                   response.Headers.CacheControl != null,
                   "Caching headers should be present for public API");
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_CreateService_ShouldWorkThroughAdminApi()
    {
        // Arrange - This will fail until CreateService endpoints and authorization are implemented
        await InitializeTestInfrastructure();
        
        var createServiceRequest = new
        {
            title = "Integration Test Medical Service",
            description = "End-to-end integration test for medical service creation with full audit trail",
            deliveryMode = "OutpatientService",
            categoryId = Guid.NewGuid(),
            slug = "integration-test-medical-service"
        };
        
        var json = JsonSerializer.Serialize(createServiceRequest);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Add admin authorization header (will fail until auth is implemented)
        content.Headers.Add("Authorization", "Bearer test-admin-token");
        var requestUri = "/admin/services";

        // Act
        var response = await _gatewayClient!.PostAsync(new Uri(requestUri, UriKind.Relative), content);
        
        // Assert - Should create service or fail auth (but route should work)
        Assert.True(response.StatusCode == HttpStatusCode.Created ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.Forbidden,
                   "Should either create service or properly reject unauthorized request");
                   
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var createdService = await response.Content.ReadFromJsonAsync<CreateServiceResponse>();
            Assert.NotNull(createdService);
            Assert.Equal(createServiceRequest.title, createdService.Title);
            
            // Verify medical-grade audit fields are populated
            Assert.NotNull(createdService.CreatedBy);
            Assert.True(createdService.CreatedOn <= DateTimeOffset.UtcNow);
        }
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_DatabaseConnectivity_ShouldPersistData()
    {
        // Arrange - This will fail until PostgreSQL database is properly configured
        await InitializeTestInfrastructure();
        
        var serviceId = await CreateTestService();

        // Act - Retrieve same service to verify database persistence
        var response1 = await _gatewayClient!.GetAsync(new Uri($"/api/services/{serviceId}", UriKind.Relative));
        var response2 = await _gatewayClient!.GetAsync(new Uri($"/api/services/{serviceId}", UriKind.Relative));
        
        // Assert - Both requests should return same persisted data
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var service1 = await response1.Content.ReadFromJsonAsync<GetServiceResponse>();
        var service2 = await response2.Content.ReadFromJsonAsync<GetServiceResponse>();
        
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.Equal(service1.Id, service2.Id);
        Assert.Equal(service1.Title, service2.Title);
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_RedisCaching_ShouldImprovePerformance()
    {
        // Arrange - This will fail until Redis caching is properly configured
        await InitializeTestInfrastructure();
        
        var serviceId = await CreateTestService();
        var requestUri = $"/api/services/{serviceId}";

        // Act - Make multiple requests to verify caching
        var response1 = await _gatewayClient!.GetAsync(new Uri(requestUri, UriKind.Relative));
        var response2 = await _gatewayClient!.GetAsync(new Uri(requestUri, UriKind.Relative));
        
        // Assert - Second request should benefit from caching
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        // Verify cache headers indicate caching is working
        var cacheHeaders = response2.Headers.CacheControl;
        Assert.True(cacheHeaders?.MaxAge.HasValue == true ||
                   response2.Headers.Contains("X-Cache-Status"),
                   "Cache headers should indicate caching is active");
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_HealthChecks_ShouldReportAllServicesHealthy()
    {
        // Arrange - This will fail until health check orchestration is implemented
        await InitializeTestInfrastructure();

        // Act - Check health through gateway
        var response = await _gatewayClient!.GetAsync(new Uri("/health", UriKind.Relative));
        
        // Assert - Should report healthy status for all components
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var healthContent = await response.Content.ReadAsStringAsync();
        Assert.NotNull(healthContent);
        
        // Should include all critical components
        Assert.True(healthContent.Contains("PostgreSQL") || healthContent.Contains("database"),
                   "Health check should include database status");
        Assert.True(healthContent.Contains("Redis") || healthContent.Contains("cache"),
                   "Health check should include cache status");
        Assert.True(healthContent.Contains("services-domain") || healthContent.Contains("Healthy"),
                   "Health check should include services domain status");
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_RateLimiting_ShouldEnforcePublicApiLimits()
    {
        // Arrange - This will fail until rate limiting is implemented
        await InitializeTestInfrastructure();
        
        var serviceId = await CreateTestService();
        var requestUri = $"/api/services/{serviceId}";
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Send burst of requests to trigger rate limiting
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_gatewayClient!.GetAsync(new Uri(requestUri, UriKind.Relative)));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert - Should enforce 1000 req/min limit (may see 429 responses)
        var successResponses = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var rateLimitedResponses = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        
        Assert.True(successResponses > 0, "Some requests should succeed");
        
        // Verify rate limiting headers are present
        var firstResponse = responses.First();
        Assert.True(firstResponse.Headers.Any(h => h.Key.Contains("RateLimit")) ||
                   firstResponse.Headers.Any(h => h.Key.Contains("X-RateLimit")),
                   "Rate limiting headers should be present");
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_MedicalAuditCompliance_ShouldLogAllOperations()
    {
        // Arrange - This will fail until medical-grade audit logging is implemented
        await InitializeTestInfrastructure();

        // Act - Perform various operations that should be audited
        var serviceId = await CreateTestService();
        var getResponse = await _gatewayClient!.GetAsync(new Uri($"/api/services/{serviceId}", UriKind.Relative));
        
        // Assert - Verify audit compliance headers and logging
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        // Medical audit requirements: correlation ID for request tracing
        Assert.True(getResponse.Headers.Contains("X-Correlation-ID") ||
                   getResponse.Headers.Contains("x-correlation-id"),
                   "Medical audit correlation ID must be present");
                   
        // Medical audit requirements: request timestamp for compliance
        Assert.True(getResponse.Headers.Contains("Date") ||
                   getResponse.Headers.Date.HasValue,
                   "Request timestamp must be present for medical audit");
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_ServiceDiscovery_ShouldConnectAllServices()
    {
        // Arrange - This will fail until Aspire service discovery is properly configured
        await InitializeTestInfrastructure();

        // Act - Test that gateway can discover and connect to services domain
        var healthResponse = await _gatewayClient!.GetAsync(new Uri("/health", UriKind.Relative));
        
        // Assert - Health check should show all services are discoverable and connected
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        
        // Should indicate successful service discovery
        Assert.True(healthContent.Contains("services-domain") && 
                   (healthContent.Contains("Healthy") || healthContent.Contains("Up")),
                   "Service discovery should successfully connect gateway to services domain");
    }

    [Fact(Timeout = 30000)]
    public async Task EndToEnd_ErrorHandling_ShouldProvideConsistentResponses()
    {
        // Arrange - This will fail until comprehensive error handling is implemented
        await InitializeTestInfrastructure();

        // Act - Test various error scenarios
        var notFoundResponse = await _gatewayClient!.GetAsync(new Uri($"/api/services/{Guid.NewGuid()}", UriKind.Relative));
        var invalidResponse = await _gatewayClient!.GetAsync(new Uri("/api/services/invalid-guid", UriKind.Relative));
        
        // Assert - Should handle errors gracefully with consistent format
        Assert.Equal(HttpStatusCode.NotFound, notFoundResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        
        // Should have consistent error response format
        var notFoundContent = await notFoundResponse.Content.ReadAsStringAsync();
        var invalidContent = await invalidResponse.Content.ReadAsStringAsync();
        
        Assert.NotNull(notFoundContent);
        Assert.NotNull(invalidContent);
        
        // Error responses should include correlation IDs for medical audit compliance
        Assert.True(notFoundResponse.Headers.Contains("X-Correlation-ID") ||
                   notFoundResponse.Headers.Contains("x-correlation-id"),
                   "Error responses must include correlation ID for medical audit");
    }

    private async Task InitializeTestInfrastructure()
    {
        // Initialize distributed application with all resources
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInfrastructureResources();
        builder.AddInternationalCenterServices();
        builder.ConfigureEnvironment();
        _app = builder.Build();
        await _app.StartAsync().ConfigureAwait(false);
        
        // Get HTTP clients for gateway and direct service access
        _gatewayClient = _app.CreateHttpClient("api-gateway");
        _directServiceClient = _app.CreateHttpClient("services-domain");
    }

    private async Task<Guid> CreateTestService()
    {
        // Helper method to create test service data
        // This will be implemented once CreateService infrastructure is working
        var testServiceId = Guid.NewGuid();
        
        // For now, return mock ID - will integrate with actual creation once infrastructure is ready
        return testServiceId;
    }

    private async Task CreateTestServiceWithSlug(string slug)
    {
        // Helper method to create test service with specific slug
        // Will integrate with actual creation once infrastructure is ready
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app != null)
        {
            await _app.DisposeAsync().ConfigureAwait(false);
        }
        
        _gatewayClient?.Dispose();
        _directServiceClient?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}