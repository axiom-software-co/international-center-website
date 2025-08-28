using Microsoft.Extensions.Configuration;
using SharedPlatform.Features.Configuration.Abstractions;

namespace SharedPlatform.Features.Configuration.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService()
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();
    }

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IConfiguration GetConfiguration()
    {
        return _configuration;
    }

    public IConfigurationSection GetSection(string sectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        return _configuration.GetSection(sectionName);
    }

    public T? GetValue<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _configuration.GetValue<T>(key);
    }

    public string? GetValue(string key, string? defaultValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _configuration.GetValue(key, defaultValue);
    }
}