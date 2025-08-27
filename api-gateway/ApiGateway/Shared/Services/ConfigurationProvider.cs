using Microsoft.Extensions.Configuration;
using ApiGateway.Shared.Abstractions;
using ApiGateway.Shared.Configuration;

namespace ApiGateway.Shared.Services;

public class ConfigurationProvider : IGatewayConfiguration
{
    private readonly IConfiguration _configuration;
    
    public ConfigurationProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string Environment => _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
    
    public bool IsPublicGateway => _configuration.GetValue<bool>("Gateway:EnablePublicApi", true);
    
    public bool IsAdminGateway => _configuration.GetValue<bool>("Gateway:EnableAdminApi", true);
    
    public string[] AllowedOrigins => _configuration.GetSection("Gateway:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    
    public TimeSpan RequestTimeout => _configuration.GetValue<TimeSpan>("Gateway:RequestTimeout", TimeSpan.FromSeconds(30));
    
    public T GetSection<T>(string sectionName) where T : class, new()
    {
        return _configuration.GetSection(sectionName).Get<T>() ?? new T();
    }
}