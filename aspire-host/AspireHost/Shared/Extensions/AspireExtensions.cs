using Aspire.Hosting;
using AspireHost.Features.ServiceDiscovery;
using AspireHost.Features.ResourceOrchestration;
using AspireHost.Features.EnvironmentManagement;
using AspireHost.Features.HealthOrchestration;
using AspireHost.Shared.Configuration;

namespace AspireHost.Shared.Extensions;

public static class AspireExtensions
{
    public static IDistributedApplicationBuilder AddInternationalCenterServices(this IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static IDistributedApplicationBuilder AddInfrastructureResources(this IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static IDistributedApplicationBuilder ConfigureEnvironment(this IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static IDistributedApplicationBuilder AddHealthOrchestration(this IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static IDistributedApplicationBuilder AddObservability(this IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}