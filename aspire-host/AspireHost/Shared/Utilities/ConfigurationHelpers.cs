using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using SharedPlatform.Features.Configuration.Options;

namespace AspireHost.Shared.Utilities;

public static class ConfigurationHelpers
{
    public static string GetEnvironmentName(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static T GetConfigurationSection<T>(IDistributedApplicationBuilder builder, string sectionName) where T : class, new()
    {
        throw new NotImplementedException();
    }

    public static string GetConnectionString(IDistributedApplicationBuilder builder, string name)
    {
        throw new NotImplementedException();
    }

    public static void SetEnvironmentVariable(IDistributedApplicationBuilder builder, string key, string value)
    {
        throw new NotImplementedException();
    }

    public static Dictionary<string, string> LoadEnvironmentConfiguration(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}