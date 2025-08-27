using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.Configuration;
using ApiGateway.Shared.Configuration;

namespace ApiGateway.Shared.Extensions;

public static class YarpExtensions
{
    public static IServiceCollection AddYarpGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));
        
        services.Configure<YarpOptions>(configuration.GetSection(YarpOptions.SectionName));
        
        return services;
    }
    
    public static IServiceCollection AddYarpRouteProvider(this IServiceCollection services)
    {
        throw new NotImplementedException();
    }
}