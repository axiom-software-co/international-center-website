using Microsoft.Extensions.Configuration;
using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.Configuration.Providers;

public class EnvironmentProvider
{
    private readonly Dictionary<string, IConfiguration> _environmentConfigurations;

    public EnvironmentProvider()
    {
        _environmentConfigurations = new Dictionary<string, IConfiguration>();
        InitializeEnvironments();
    }

    public Result<IConfiguration> GetEnvironmentConfiguration(string environment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);

        if (_environmentConfigurations.TryGetValue(environment, out var configuration))
        {
            return Result<IConfiguration>.Success(configuration);
        }

        return Error.NotFound("EnvironmentProvider.GetEnvironmentConfiguration", $"Configuration for environment '{environment}' not found");
    }

    private void InitializeEnvironments()
    {
        var validEnvironments = new[] { "Development", "Testing", "Production" };

        foreach (var env in validEnvironments)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddInMemoryCollection(GetDefaultConfigurationForEnvironment(env))
                .Build();

            _environmentConfigurations[env] = configuration;
        }
    }

    private static Dictionary<string, string?> GetDefaultConfigurationForEnvironment(string environment)
    {
        return environment switch
        {
            "Development" => new Dictionary<string, string?>
            {
                ["Environment"] = "Development",
                ["Logging:LogLevel:Default"] = "Information",
                ["ConnectionStrings:DefaultConnection"] = "Development Connection String",
                ["FeatureFlags:DevFeature"] = "true"
            },
            "Testing" => new Dictionary<string, string?>
            {
                ["Environment"] = "Testing",
                ["Logging:LogLevel:Default"] = "Warning",
                ["ConnectionStrings:DefaultConnection"] = "Testing Connection String",
                ["FeatureFlags:DevFeature"] = "false"
            },
            "Production" => new Dictionary<string, string?>
            {
                ["Environment"] = "Production",
                ["Logging:LogLevel:Default"] = "Error",
                ["ConnectionStrings:DefaultConnection"] = "Production Connection String",
                ["FeatureFlags:DevFeature"] = "false",
                ["FeatureFlags:ProductionFeature"] = "true"
            },
            _ => new Dictionary<string, string?>()
        };
    }
}