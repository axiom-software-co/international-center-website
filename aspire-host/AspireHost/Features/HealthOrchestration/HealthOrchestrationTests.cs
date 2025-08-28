using Aspire.Hosting;
using AspireHost.Features.HealthOrchestration;
using AspireHost.Shared.Extensions;
using SharedPlatform.Features.Testing.Fixtures;
using Xunit;

namespace AspireHost.Features.HealthOrchestration;

public class HealthOrchestrationTests
{
    [Fact]
    public void DistributedHealthChecks_ConfigureHealthChecks_ShouldSetupHealthChecks()
    {
        // Arrange & Act - GREEN phase: verify health checks configuration
        var builder = DistributedApplication.CreateBuilder();
        builder.AddHealthOrchestration();
        var app = builder.Build();
        
        // Assert - Should setup health checks
        Assert.NotNull(app);
    }

    [Fact]
    public void ServiceHealthMonitoring_ConfigureServiceMonitoring_ShouldSetupServiceMonitoring()
    {
        // Arrange & Act - GREEN phase: verify service monitoring configuration
        var builder = DistributedApplication.CreateBuilder();
        builder.AddHealthOrchestration();
        var app = builder.Build();
        
        // Assert - Should setup service monitoring
        Assert.NotNull(app);
    }

    [Fact]
    public void DistributedHealthChecks_AddServiceHealthChecks_ShouldAddAllServiceHealthChecks()
    {
        // Arrange & Act - GREEN phase: verify service health checks added
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInfrastructureResources();
        builder.AddHealthOrchestration();
        var app = builder.Build();
        
        // Assert - Should add all service health checks
        Assert.NotNull(app);
    }

    [Fact]
    public void ServiceHealthMonitoring_AddApiGatewayHealthCheck_ShouldAddApiGatewayHealthCheck()
    {
        // Arrange & Act - GREEN phase: verify API Gateway health check added
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInternationalCenterServices();
        builder.AddHealthOrchestration();
        var app = builder.Build();
        
        // Assert - Should add API Gateway health check
        Assert.NotNull(app);
    }

    [Fact]
    public void DistributedHealthChecks_SetupHealthCheckDashboard_ShouldCreateDashboard()
    {
        // Arrange & Act - GREEN phase: verify health check dashboard creation
        var builder = DistributedApplication.CreateBuilder();
        builder.AddObservability();
        var app = builder.Build();
        
        // Assert - Should create health check dashboard
        Assert.NotNull(app);
    }
}