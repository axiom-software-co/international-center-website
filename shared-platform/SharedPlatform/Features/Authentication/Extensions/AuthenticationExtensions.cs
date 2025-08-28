using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedPlatform.Features.Authentication.Abstractions;
using SharedPlatform.Features.Authentication.Configuration;
using SharedPlatform.Features.Authentication.Services;

namespace SharedPlatform.Features.Authentication.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IUserContext, UserContextService>();
        
        services.AddHttpContextAccessor();
        
        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, Action<JwtOptions> configureJwt)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureJwt);

        services.Configure(configureJwt);
        
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IUserContext, UserContextService>();
        
        services.AddHttpContextAccessor();
        
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        
        return services;
    }

    public static IServiceCollection AddAnonymousAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<IAuthenticationService, AnonymousAuthService>();
        
        return services;
    }

    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<IUserContext, UserContextService>();
        services.AddHttpContextAccessor();
        
        return services;
    }
}