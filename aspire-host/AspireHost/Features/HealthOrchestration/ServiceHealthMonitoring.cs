using Aspire.Hosting;
using SharedPlatform.Features.Observability.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspireHost.Features.HealthOrchestration;

public class ServiceHealthMonitoring
{
    public static void ConfigureServiceMonitoring(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void AddApiGatewayHealthCheck(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void AddServicesDomainHealthCheck(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureHealthCheckPolicies(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void SetupHealthAlerts(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}