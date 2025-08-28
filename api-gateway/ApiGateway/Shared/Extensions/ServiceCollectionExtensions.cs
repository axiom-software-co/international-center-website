using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using ApiGateway.Shared.Abstractions;
using ApiGateway.Shared.Services;
using ApiGateway.Shared.Configuration;
using ApiGateway.Features.Authentication;
using ApiGateway.Features.Authorization;
using ApiGateway.Features.RateLimiting;
using ApiGateway.Features.Security;
using ApiGateway.Features.ErrorHandling;
using ApiGateway.Features.Observability;
using ApiGateway.Features.HealthChecks;
using System.IO.Compression;

namespace ApiGateway.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly string[] AdditionalMimeTypes = 
    {
        "application/json",
        "application/javascript",
        "text/css",
        "text/html",
        "text/json",
        "text/plain"
    };
    public static IServiceCollection AddGatewayCore(this IServiceCollection services, IConfiguration configuration)
    {
        // REFACTOR PHASE: Medical-grade performance optimization and core services
        
        // High-performance YARP with optimized configuration
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));
        
        // Medical-grade performance: Response compression for reduced bandwidth
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(AdditionalMimeTypes);
        });
        
        // Configure compression levels for optimal performance
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest; // Balance compression vs CPU
        });
        
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });
        
        // Medical-grade performance: Response caching with aggressive caching
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 64 * 1024 * 1024; // 64MB
            options.UseCaseSensitivePaths = false;
            options.SizeLimit = 200 * 1024 * 1024; // 200MB total cache
        });
        
        // Medical-grade performance: Memory caching for frequently accessed data
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100MB
            options.CompactionPercentage = 0.25; // Compact when 75% full
            options.TrackLinkedCacheEntries = false; // Performance optimization
        });
        
        // Medical-grade performance: HTTP client factory with connection pooling
        services.AddHttpClient("default").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = 100, // High concurrency support
            UseCookies = false, // Performance optimization for API gateway
        });
        
        return services;
    }
    
    public static IServiceCollection AddGatewayConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // REFACTOR PHASE: Medical-grade configuration with performance optimizations
        
        // Bind configurations with high-performance options pattern
        services.Configure<GatewayOptions>(configuration.GetSection("Gateway"));
        services.Configure<PublicGatewayOptions>(configuration.GetSection("Gateway:Public"));
        services.Configure<AdminGatewayOptions>(configuration.GetSection("Gateway:Admin"));
        services.Configure<MiddlewareOptions>(configuration.GetSection("Gateway:Middleware"));
        services.Configure<YarpOptions>(configuration.GetSection("ReverseProxy"));
        
        // Medical-grade security configuration
        services.Configure<RateLimitConfiguration>(configuration.GetSection("RateLimiting"));
        services.Configure<SecurityConfiguration>(configuration.GetSection("Security"));
        services.Configure<ErrorConfiguration>(configuration.GetSection("ErrorHandling"));
        services.Configure<ObservabilityConfiguration>(configuration.GetSection("Observability"));
        services.Configure<HealthCheckConfiguration>(configuration.GetSection("HealthChecks"));
        
        return services;
    }
    
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        // REFACTOR PHASE: Medical-grade services with performance optimizations
        
        // Core gateway services (singletons for performance)
        services.AddSingleton<IGatewayService, GatewayService>();
        services.AddSingleton<Services.ConfigurationProvider>();
        
        // Scoped services for request context
        services.AddScoped<GatewayContextService>();
        services.AddScoped<MiddlewarePipeline>();
        
        // Authentication strategies (singletons for performance)
        services.AddSingleton<IAuthenticationStrategy, AnonymousStrategy>();
        services.AddSingleton<IAuthenticationStrategy, JwtStrategy>();
        services.AddSingleton<IAuthenticationStrategy, EntraIdStrategy>();
        
        // Authorization strategies (singletons for performance)
        services.AddSingleton<IAuthorizationStrategy, PublicAuthorizationStrategy>();
        services.AddSingleton<IAuthorizationStrategy, AdminAuthorizationStrategy>();
        
        // Rate limiting services
        services.AddSingleton<RateLimitConfiguration>();
        services.AddSingleton<RedisRateLimitStore>();
        services.AddSingleton<IpBasedRateLimiter>();
        services.AddSingleton<UserBasedRateLimiter>();
        services.AddSingleton<RateLimitingService>();
        
        // Security services (singletons for performance)
        services.AddSingleton<SecurityConfiguration>();
        
        // Error handling services
        services.AddSingleton<ErrorConfiguration>();
        services.AddSingleton<ErrorResponseFormatter>();
        services.AddSingleton<GatewayErrorHandler>();
        
        // Observability services
        services.AddSingleton<ObservabilityConfiguration>();
        services.AddSingleton<GatewayMetrics>();
        
        // Health check services
        services.AddSingleton<HealthCheckConfiguration>();
        services.AddSingleton<GatewayHealthCheck>();
        services.AddSingleton<HealthCheckService>();
        
        return services;
    }
}