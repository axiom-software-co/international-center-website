using Aspire.Hosting;
using SharedPlatform.Features.Configuration.Options;

namespace AspireHost.Features.ServiceDiscovery;

public class ServiceDiscoveryConfiguration
{
    public string ConsulEndpoint { get; set; } = string.Empty;
    public bool EnableHealthChecks { get; set; } = true;
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, string> ServiceTags { get; set; } = new();

    public static ServiceDiscoveryConfiguration Configure(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void ApplyConfiguration(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void RegisterHealthChecks(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}