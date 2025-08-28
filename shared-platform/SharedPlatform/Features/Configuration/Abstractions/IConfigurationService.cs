using Microsoft.Extensions.Configuration;

namespace SharedPlatform.Features.Configuration.Abstractions;

public interface IConfigurationService
{
    IConfiguration GetConfiguration();
    IConfigurationSection GetSection(string sectionName);
    T? GetValue<T>(string key);
    string? GetValue(string key, string? defaultValue);
}