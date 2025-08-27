using Aspire.Hosting;
using SharedPlatform.Features.Configuration.Options;

namespace AspireHost.Shared.Configuration;

public class EnvironmentConfiguration
{
    public string EnvironmentName { get; set; } = string.Empty;
    public bool IsProduction => EnvironmentName.Equals("Production", StringComparison.OrdinalIgnoreCase);
    public bool IsDevelopment => EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);
    public bool IsTesting => EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase);

    public static EnvironmentConfiguration Create(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void ConfigureEnvironment(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void SetEnvironmentVariables(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }

    public void ApplyEnvironmentSpecificSettings(IDistributedApplicationBuilder builder)
    {
        throw new NotImplementedException();
    }
}