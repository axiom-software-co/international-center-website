using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ApiGateway.Shared.Abstractions;
using ApiGateway.Shared.Services;
using ApiGateway.Shared.Configuration;

namespace ApiGateway.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayCore(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
    
    public static IServiceCollection AddGatewayConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
    
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        services.AddScoped<IGatewayService, GatewayService>();
        services.AddScoped<GatewayContextService>();
        services.AddScoped<Services.ConfigurationProvider>();
        services.AddScoped<MiddlewarePipeline>();
        return services;
    }
}