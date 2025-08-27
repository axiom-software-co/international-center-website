using Aspire.Hosting;
using SharedPlatform.Features.Caching.Configuration;

namespace AspireHost.Features.ResourceOrchestration;

public class CacheResource
{
    public static void AddRedis(IDistributedApplicationBuilder builder, string name = "redis")
    {
        throw new NotImplementedException();
    }

    public static void ConfigureCacheOptions(IDistributedApplicationBuilder builder, CacheOptions options)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureRedisSettings(IDistributedApplicationBuilder builder, RedisOptions options)
    {
        throw new NotImplementedException();
    }

    public static void AddMemoryCache(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureHybridCache(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}