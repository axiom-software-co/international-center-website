using Microsoft.Extensions.Configuration;
using ApiGateway.Shared.Configuration;

namespace ApiGateway.Shared.Extensions;

public static class ConfigurationExtensions
{
    public static T GetGatewaySection<T>(this IConfiguration configuration, string sectionName) where T : class, new()
    {
        throw new NotImplementedException();
    }
    
    public static GatewayOptions GetGatewayOptions(this IConfiguration configuration)
    {
        return configuration.GetSection(GatewayOptions.SectionName).Get<GatewayOptions>() ?? new GatewayOptions();
    }
    
    public static AdminGatewayOptions GetAdminGatewayOptions(this IConfiguration configuration)
    {
        return configuration.GetSection(AdminGatewayOptions.SectionName).Get<AdminGatewayOptions>() ?? new AdminGatewayOptions();
    }
    
    public static PublicGatewayOptions GetPublicGatewayOptions(this IConfiguration configuration)
    {
        return configuration.GetSection(PublicGatewayOptions.SectionName).Get<PublicGatewayOptions>() ?? new PublicGatewayOptions();
    }
}