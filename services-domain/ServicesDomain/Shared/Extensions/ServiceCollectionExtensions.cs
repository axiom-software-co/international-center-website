using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServicesDomain.Features.GetService;
using ServicesDomain.Features.GetServiceBySlug;
using ServicesDomain.Features.CreateService;
using ServicesDomain.Features.ServiceManagement.Domain.Repository;
using ServicesDomain.Shared.Configuration;
using ServicesDomain.Shared.Infrastructure.Data;
using ServicesDomain.Shared.Infrastructure.Repositories;
using SharedPlatform.Features.Caching.Abstractions;
using SharedPlatform.Features.Caching.Services;

namespace ServicesDomain.Shared.Extensions;

public static class ServiceCollectionExtensions 
{
    private static readonly string[] RedisHealthCheckTags = { "cache", "redis" };
 
    public static IServiceCollection AddServicesDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration - PostgreSQL with EF Core for medical-grade persistence
        // Phase 3.4: Enhanced with connection pooling and performance optimizations
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection connection string is required");
            
        // Connection pooling settings for high-throughput medical systems
        var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        if (!connectionStringBuilder.ContainsKey("Pooling"))
            connectionStringBuilder.Pooling = true;
        if (!connectionStringBuilder.ContainsKey("MinPoolSize"))
            connectionStringBuilder.MinPoolSize = 5;  // Minimum connections for medical availability
        if (!connectionStringBuilder.ContainsKey("MaxPoolSize"))
            connectionStringBuilder.MaxPoolSize = 100; // Maximum connections for high load
        if (!connectionStringBuilder.ContainsKey("ConnectionLifetime"))
            connectionStringBuilder.ConnectionLifetime = 300; // 5 minutes for medical data integrity
            
        var enhancedConnectionString = connectionStringBuilder.ToString();
        
        services.AddDbContextPool<ServicesDbContext>(options =>
        {
            options.UseNpgsql(enhancedConnectionString, npgsqlOptions =>
            {
                // Connection resilience for medical-grade reliability
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                
                // Performance optimizations
                npgsqlOptions.CommandTimeout(30); // 30 second timeout for medical queries
            });
            
            // Enable sensitive data logging in development only
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            
            // Configure query behavior for production performance
            options.ConfigureWarnings(warnings =>
            {
                warnings.Default(WarningBehavior.Throw);
            });
            
            // Performance optimizations for medical-grade systems
            options.EnableServiceProviderCaching(); // Cache service provider for performance
            options.EnableSensitiveDataLogging(false); // Disable in production for security
            
        }, poolSize: 128); // EF Core connection pool size for high-throughput medical APIs
        
        // Register DbContext interface for testing abstraction
        services.AddScoped<IServicesDbContext>(provider => provider.GetRequiredService<ServicesDbContext>());
        
        // GetService feature - Production-ready registrations
        services.AddScoped<GetServiceHandler>();
        services.AddScoped<IValidator<GetServiceQuery>, GetServiceQueryValidator>();
        
        // GetServiceBySlug feature - Public API with Dapper optimization
        services.AddScoped<GetServiceBySlugHandler>();
        services.AddScoped<IValidator<GetServiceBySlugQuery>, GetServiceBySlugValidator>();
        
        // CreateService feature - Admin API with EF Core persistence and medical-grade audit
        services.AddScoped<CreateServiceHandler>();
        services.AddScoped<IValidator<CreateServiceCommand>, CreateServiceValidator>();
        
        // Repository - EF Core implementation for admin APIs (medical-grade audit trails)
        services.AddScoped<IServiceRepository, EfServiceRepository>();
        
        // Cache configuration - Redis with in-memory fallback for medical-grade reliability
        var cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();
        cacheOptions.Validate();
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        
        // Memory cache configuration (used as fallback)
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = cacheOptions.MemoryCacheSizeLimit;
            options.CompactionPercentage = cacheOptions.MemoryCacheCompactionPercentage;
        });
        
        // Redis distributed cache configuration
        if (cacheOptions.UseDistributedCache && !string.IsNullOrWhiteSpace(cacheOptions.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisConnectionString;
                options.InstanceName = "ServicesApi";
                
                // Redis-specific configuration for medical-grade systems
                var connectionString = cacheOptions.RedisConnectionString;
                if (!connectionString.Contains("connectTimeout"))
                {
                    connectionString += $",connectTimeout={cacheOptions.RedisConnectionTimeoutSeconds * 1000}";
                }
                if (!connectionString.Contains("connectRetry"))
                {
                    connectionString += $",connectRetry={cacheOptions.RedisRetryCount}";
                }
                
                options.Configuration = connectionString;
            });
            
            // Register cache service implementations
            services.AddScoped<RedisCacheService>();
            services.AddScoped<MemoryCacheService>();
            services.AddScoped<ICacheService, HybridCacheService>();
            
            // Health check for Redis connectivity
            services.AddHealthChecks()
                .AddRedis(cacheOptions.RedisConnectionString, "redis_cache",
                    tags: RedisHealthCheckTags);
        }
        else
        {
            // Fallback to memory cache only for development/testing
            services.AddScoped<ICacheService, MemoryCacheService>();
        }
        
        // FluentValidation configuration
        services.AddValidatorsFromAssemblyContaining<GetServiceQueryValidator>(ServiceLifetime.Scoped);
        services.AddValidatorsFromAssemblyContaining<GetServiceBySlugValidator>(ServiceLifetime.Scoped);
        services.AddValidatorsFromAssemblyContaining<CreateServiceValidator>(ServiceLifetime.Scoped);
        
        // Health checks for database connectivity
        services.AddHealthChecks()
            .AddDbContextCheck<ServicesDbContext>("services_database", 
                customTestQuery: async (context, cancellationToken) =>
                {
                    // Test database connectivity with a simple query
                    return await context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
                });
        
        return services;
    }
}
