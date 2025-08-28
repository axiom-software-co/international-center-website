using Microsoft.Extensions.Configuration;
using SharedPlatform.Features.Configuration.Options;

namespace SharedPlatform.Features.Configuration.Services;

public class FeatureFlagService
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, FeatureFlag> _featureFlags;

    public FeatureFlagService()
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        
        _featureFlags = LoadFeatureFlagsFromConfiguration();
    }

    public FeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
        _featureFlags = LoadFeatureFlagsFromConfiguration();
    }

    public bool IsEnabled(string flagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flagName);
        
        if (_featureFlags.TryGetValue(flagName, out var flag))
        {
            if (flag.ExpiryDate.HasValue && flag.ExpiryDate.Value < DateTime.UtcNow)
                return false;
                
            return flag.IsEnabled;
        }

        var configValue = _configuration.GetValue<bool?>($"FeatureFlags:{flagName}");
        return configValue ?? false;
    }

    public bool IsEnabled(string flagName, Dictionary<string, object> context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flagName);
        ArgumentNullException.ThrowIfNull(context);

        if (!IsEnabled(flagName))
            return false;

        if (_featureFlags.TryGetValue(flagName, out var flag) && flag.Context != null)
        {
            foreach (var contextItem in flag.Context)
            {
                if (context.TryGetValue(contextItem.Key, out var contextValue))
                {
                    if (!contextValue.Equals(contextItem.Value))
                        return false;
                }
            }
        }

        return true;
    }

    public IEnumerable<FeatureFlag> GetFlags()
    {
        return _featureFlags.Values.ToList();
    }

    private Dictionary<string, FeatureFlag> LoadFeatureFlagsFromConfiguration()
    {
        var flags = new Dictionary<string, FeatureFlag>();
        var section = _configuration.GetSection("FeatureFlags");
        
        foreach (var child in section.GetChildren())
        {
            if (bool.TryParse(child.Value, out var isEnabled))
            {
                flags[child.Key] = new FeatureFlag
                {
                    Name = child.Key,
                    IsEnabled = isEnabled
                };
            }
        }

        return flags;
    }
}