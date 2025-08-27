using Aspire.Hosting;
using SharedPlatform.Features.Configuration.Options;

namespace AspireHost.Shared.Configuration;

public class AspireConfiguration
{
    public string ApplicationName { get; set; } = "InternationalCenter";
    public string Environment { get; set; } = "Development";
    public Dictionary<string, string> ServiceUrls { get; set; } = new();
    public Dictionary<string, object> ResourceSettings { get; set; } = new();

    public static AspireConfiguration Load(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void ApplyConfiguration(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void ConfigureServices(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void ConfigureResources(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}