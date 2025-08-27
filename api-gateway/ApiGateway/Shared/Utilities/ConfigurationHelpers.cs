using Microsoft.Extensions.Configuration;

namespace ApiGateway.Shared.Utilities;

public static class ConfigurationHelpers
{
    public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
    {
        throw new NotImplementedException();
    }
    
    public static T GetValueOrDefault<T>(this IConfiguration configuration, string key, T defaultValue)
    {
        throw new NotImplementedException();
    }
    
    public static bool HasSection(this IConfiguration configuration, string sectionName)
    {
        return configuration.GetSection(sectionName).Exists();
    }
    
    public static Dictionary<string, string> GetConnectionStrings(this IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}