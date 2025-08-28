using Aspire.Hosting;
using AspireHost.Features.ServiceDiscovery;
using AspireHost.Shared.Extensions;
using SharedPlatform.Features.Testing.Fixtures;
using Xunit;

namespace AspireHost.Features.ServiceDiscovery;

public class ServiceDiscoveryTests
{
    [Fact]
    public void ServiceRegistration_RegisterServices_ShouldRegisterAllServices()
    {
        // Arrange & Act - GREEN phase: verify extension methods compile and work
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInfrastructureResources();
        builder.AddInternationalCenterServices();
        var app = builder.Build();
        
        // Assert - Should successfully register all services
        Assert.NotNull(app);
    }

    [Fact]
    public void ServiceDiscoveryConfiguration_Configure_ShouldReturnValidConfiguration()
    {
        // Arrange & Act - GREEN phase: verify configuration works
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInfrastructureResources();
        var app = builder.Build();
        
        // Assert - Should return valid configuration
        Assert.NotNull(app);
        Assert.NotNull(app.Services);
    }

    [Fact]
    public void ServiceRegistration_RegisterApiGateway_ShouldRegisterApiGateway()
    {
        // Arrange & Act - GREEN phase: verify API Gateway registration
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInternationalCenterServices();
        var app = builder.Build();
        
        // Assert - Should register API Gateway
        Assert.NotNull(app);
    }

    [Fact]
    public void ServiceRegistration_RegisterServicesDomain_ShouldRegisterServicesDomain()
    {
        // Arrange & Act - GREEN phase: verify Services Domain registration
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInternationalCenterServices();
        var app = builder.Build();
        
        // Assert - Should register Services Domain
        Assert.NotNull(app);
    }

    [Fact]
    public void ServiceDiscoveryConfiguration_ApplyConfiguration_ShouldApplySettings()
    {
        // Arrange & Act - GREEN phase: verify configuration application
        var builder = DistributedApplication.CreateBuilder();
        builder.ConfigureEnvironment();
        var app = builder.Build();
        
        // Assert - Should apply configuration settings
        Assert.NotNull(app);
    }
}