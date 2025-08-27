using Aspire.Hosting;
using SharedPlatform.Features.Observability.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspireHost.Features.HealthOrchestration;

public class DistributedHealthChecks
{
    public static void ConfigureHealthChecks(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void AddServiceHealthChecks(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void AddResourceHealthChecks(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureHealthCheckEndpoints(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void SetupHealthCheckDashboard(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}