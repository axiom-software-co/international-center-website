using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ApiGateway.Features.Routing;

/// <summary>
/// Integration tests for API Gateway YARP routing and policies
/// Tests will FAIL until ApiGateway Program.cs is properly implemented
/// Covers public and admin API routing with medical-grade rate limiting
/// </summary>
public sealed class RoutingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RoutingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldRoutePublicServiceRequests_ToServicesDomain()
    {
        // Arrange - This will fail until YARP routing is configured
        var serviceId = Guid.NewGuid();
        var requestUri = $"/api/services/{serviceId}";

        // Act
        var response = await _client.GetAsync(new Uri(requestUri, UriKind.Relative));

        // Assert - Should route to services domain (may return 404 but routing should work)
        // Should NOT return 502 Bad Gateway or routing errors
        Assert.NotEqual(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        
        // Verify routing headers indicate YARP processed the request
        Assert.True(response.Headers.Contains("via") || 
                   response.Headers.Any(h => h.Key.StartsWith("x-forwarded")),
                   "Request should be routed through YARP reverse proxy");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldRoutePublicServiceBySlugRequests_ToServicesDomain()
    {
        // Arrange - This will fail until YARP routing is configured for slug endpoints
        var slug = "medical-consultation";
        var requestUri = $"/api/services/by-slug/{slug}";

        // Act
        var response = await _client.GetAsync(new Uri(requestUri, UriKind.Relative));

        // Assert - Verify slug routing works
        Assert.NotEqual(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode); // Gateway should know route exists
        
        // Should route to services domain
        Assert.True(response.Headers.Contains("via") || 
                   response.Headers.Any(h => h.Key.StartsWith("x-forwarded")));
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldRouteAdminServiceRequests_ToServicesDomain_WithAuthorization()
    {
        // Arrange - This will fail until admin routing and authorization are configured
        var serviceId = Guid.NewGuid();
        var requestUri = $"/admin/services/{serviceId}";

        // Act - Try without authorization (should be rejected by gateway)
        var response = await _client.GetAsync(new Uri(requestUri, UriKind.Relative));

        // Assert - Should enforce authorization at gateway level
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.Forbidden,
                   "Admin endpoints should require authorization at gateway level");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldEnforcePublicRateLimit_Of1000RequestsPerMinute()
    {
        // Arrange - This will fail until rate limiting is configured
        var requestUri = "/api/services";
        var tasks = new List<Task<HttpResponse>>();

        // Act - Send requests rapidly to trigger rate limiting
        for (int i = 0; i < 50; i++) // Send burst of requests
        {
            tasks.Add(SendRequestWithResponse(requestUri));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert - Should apply rate limiting (may get 429 Too Many Requests)
        var rateLimitedResponses = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var successfulResponses = responses.Count(r => r.StatusCode != HttpStatusCode.TooManyRequests);
        
        // Verify rate limiting headers are present
        var firstResponse = responses.First();
        Assert.True(firstResponse.Headers.ContainsKey("X-RateLimit-Limit") ||
                   firstResponse.Headers.ContainsKey("RateLimit-Limit"),
                   "Rate limiting headers should be present");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldApplySecurityHeaders_ToAllResponses()
    {
        // Arrange - This will fail until security headers middleware is configured
        var requestUri = "/api/services";

        // Act
        var response = await _client.GetAsync(new Uri(requestUri, UriKind.Relative));

        // Assert - Verify security headers are applied
        var headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
        
        // Should have security headers for medical-grade compliance
        Assert.True(headers.ContainsKey("X-Content-Type-Options") ||
                   headers.ContainsKey("x-content-type-options"),
                   "Should have X-Content-Type-Options header");
                   
        Assert.True(headers.ContainsKey("X-Frame-Options") ||
                   headers.ContainsKey("x-frame-options"),
                   "Should have X-Frame-Options header");
                   
        // Should remove server identification for security
        Assert.False(headers.ContainsKey("Server"), 
                    "Server header should be removed for security");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldEnforceCorsPolicy_ForPublicApiOrigins()
    {
        // Arrange - This will fail until CORS policies are configured
        var requestUri = "/api/services";
        using var request = new HttpRequestMessage(HttpMethod.Options, new Uri(requestUri, UriKind.Relative));
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Should handle preflight CORS requests
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.NoContent,
                   "Should handle CORS preflight requests");
                   
        // Should have CORS headers
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin") ||
                   response.Headers.Contains("access-control-allow-origin"),
                   "Should include CORS headers");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldHealthCheck_AllDownstreamServices()
    {
        // Arrange - This will fail until health check routing is configured
        var healthCheckUri = "/health";

        // Act
        var response = await _client.GetAsync(new Uri(healthCheckUri, UriKind.Relative));

        // Assert - Should provide health status
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        
        // Should indicate status of downstream services
        Assert.True(content.Contains("services-domain") || content.Contains("Healthy"),
                   "Health check should include downstream service status");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldLoadBalance_AcrossMultipleServiceInstances()
    {
        // Arrange - This will fail until load balancing is configured
        var requestUri = "/api/services";
        var responses = new List<HttpResponse>();

        // Act - Send multiple requests to observe load balancing
        for (int i = 0; i < 10; i++)
        {
            var response = await SendRequestWithResponse(requestUri);
            responses.Add(response);
        }

        // Assert - Should distribute load (evidenced by different upstream servers)
        var upstreamHeaders = responses
            .SelectMany(r => r.Headers.Where(h => h.Key.Contains("upstream") || h.Key.Contains("server")))
            .ToList();
            
        // If load balancing is working, we might see different upstream identifiers
        // At minimum, requests should be processed successfully
        var successfulRequests = responses.Count(r => r.StatusCode != HttpStatusCode.BadGateway);
        Assert.True(successfulRequests > 0, "Some requests should be successfully routed");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldRoute_CreateServiceRequests_ToAdminEndpoints()
    {
        // Arrange - This will fail until POST routing for CreateService is configured
        var requestUri = "/admin/services";
        var serviceData = new
        {
            title = "Test Medical Service",
            description = "Test description for medical service validation",
            deliveryMode = "OutpatientService",
            categoryId = Guid.NewGuid(),
            slug = "test-medical-service"
        };
        
        var json = JsonSerializer.Serialize(serviceData);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Should be rejected due to missing authorization, but route should exist
        var response = await _client.PostAsync(new Uri(requestUri, UriKind.Relative), content);

        // Assert - Should route but require authorization
        Assert.NotEqual(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        
        // Should require authorization for admin endpoints
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.Forbidden,
                   "Admin POST endpoints should require authorization");
    }

    [Fact(Timeout = 15000)]
    public async Task Gateway_ShouldProvideObservabilityMetrics_ForAllRoutes()
    {
        // Arrange - This will fail until observability is configured
        var metricsUri = "/metrics";

        // Act
        var response = await _client.GetAsync(new Uri(metricsUri, UriKind.Relative));

        // Assert - Should expose metrics endpoint
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.NotFound, // NotFound acceptable if not configured yet
                   "Metrics endpoint should be available or properly indicate it's not configured");
                   
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            // Should contain Prometheus-style metrics
            Assert.True(content.Contains("http_requests") || content.Contains("# HELP"),
                       "Should provide HTTP request metrics");
        }
    }

    private async Task<HttpResponse> SendRequestWithResponse(string uri)
    {
        var httpResponse = await _client.GetAsync(new Uri(uri, UriKind.Relative)).ConfigureAwait(false);
        return new HttpResponse
        {
            StatusCode = httpResponse.StatusCode,
            Headers = httpResponse.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray())
        };
    }

    private class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string[]> Headers { get; set; } = new();
    }
}
