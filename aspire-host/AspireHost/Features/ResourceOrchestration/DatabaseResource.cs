using Aspire.Hosting;
using SharedPlatform.Features.DataAccess.Configuration;

namespace AspireHost.Features.ResourceOrchestration;

public class DatabaseResource
{
    public static void AddPostgreSql(IDistributedApplicationBuilder builder, string name = "postgres")
    {
        throw new NotImplementedException();
    }

    public static void AddDatabase(IDistributedApplicationBuilder builder, string name, string connectionString)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureDatabaseOptions(IDistributedApplicationBuilder builder, DatabaseOptions options)
    {
        throw new NotImplementedException();
    }

    public static void AddMigrations(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public static void ConfigureConnectionStrings(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}