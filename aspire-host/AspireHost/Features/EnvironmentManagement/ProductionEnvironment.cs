using Aspire.Hosting;
using AspireHost.Features.ResourceOrchestration;
using SharedPlatform.Features.Configuration.Options;
using SharedPlatform.Features.Security.Configuration;

namespace AspireHost.Features.EnvironmentManagement;

public class ProductionEnvironment
{
    public static void Configure(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void SetupProductionResources(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureProductionSettings(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void EnableSecurityFeatures(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureScaling(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}