using Aspire.Hosting;
using SharedPlatform.Shared.Utilities;

namespace AspireHost.Shared.Utilities;

public static class ResourceHelpers
{
    public static string GenerateResourceName(string baseName, string environment)
    {
        throw new NotImplementedException();
    }

    public static Dictionary<string, object> CreateResourceOptions(string environment)
    {
        throw new NotImplementedException();
    }

    public static void ApplyResourceTags(IDistributedApplicationBuilder builder, Dictionary<string, string> tags)
    {
        throw new NotImplementedException();
    }

    public static bool IsResourceHealthy(string resourceName)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureResourceForEnvironment(IDistributedApplicationBuilder builder, string environment)
    {
        throw new NotImplementedException();
    }
}