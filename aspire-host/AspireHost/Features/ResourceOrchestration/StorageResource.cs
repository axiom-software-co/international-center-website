using Aspire.Hosting;
using SharedPlatform.Features.Configuration.Options;

namespace AspireHost.Features.ResourceOrchestration;

public class StorageResource
{
    public static void AddBlobStorage(IDistributedApplicationBuilder builder, string name = "storage")
    {
        throw new NotImplementedException();
    }

    public static void ConfigureStorageOptions(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void AddFileStorage(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureAzureStorage(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void AddLocalStorage(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}