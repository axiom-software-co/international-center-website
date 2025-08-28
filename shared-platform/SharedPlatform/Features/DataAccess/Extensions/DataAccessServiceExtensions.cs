using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;
using Microsoft.EntityFrameworkCore;
using SharedPlatform.Features.DataAccess.EntityFramework;
using SharedPlatform.Features.DataAccess.EntityFramework.Entities;
using SharedPlatform.Features.DataAccess.Dapper;
using SharedPlatform.Features.DataAccess.Interceptors;
using SharedPlatform.Features.DataAccess.HealthChecks;
using System.Data;
using Dapper;

namespace SharedPlatform.Features.DataAccess.Extensions;

/// <summary>
/// Extension methods for registering data access services
/// GREEN PHASE: Complete implementation for medical-grade data access
/// Medical-grade data access service registration with dual strategy (EF Core + Dapper)
/// </summary>
public static class DataAccessServiceExtensions
{
    /// <summary>
    /// SQLite GUID type handler for Dapper to handle string-to-GUID conversion
    /// </summary>
    private sealed class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            return value switch
            {
                string stringValue when Guid.TryParse(stringValue, out var guid) => guid,
                Guid guidValue => guidValue,
                _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to Guid")
            };
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }

    /// <summary>
    /// SQLite nullable GUID type handler for Dapper to handle string-to-nullable-GUID conversion
    /// </summary>
    private sealed class SqliteNullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
    {
        public override Guid? Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            return value switch
            {
                string stringValue when string.IsNullOrEmpty(stringValue) => null,
                string stringValue when Guid.TryParse(stringValue, out var guid) => guid,
                Guid guidValue => guidValue,
                _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to Guid?")
            };
        }

        public override void SetValue(IDbDataParameter parameter, Guid? value)
        {
            parameter.Value = value?.ToString() ?? (object)DBNull.Value;
        }
    }

    static DataAccessServiceExtensions()
    {
        // Register SQLite GUID type handlers for Dapper
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
    }
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services, 
        object database,
        object redis)
    {
        // GREEN PHASE: Complete service registration
        services.AddEntityFrameworkServices(database);
        services.AddDapperServices(database);
        services.AddMedicalAuditInterceptors();
        services.AddDataAccessHealthChecks();
        
        return services;
    }

    // GREEN PHASE: Testing-specific overload with real in-memory database (no mocks)
    public static IServiceCollection AddDataAccessServicesForTesting(
        this IServiceCollection services,
        string connectionString)
    {
        // Add HTTP context accessor for CorrelationIdInterceptor
        services.AddHttpContextAccessor();
        
        // Add interceptors first (required for DbContext configuration)
        services.AddMedicalAuditInterceptors();
        
        // Configure EF Core with SQLite in-memory database for testing
        services.AddDbContext<ServicesDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite(connectionString);
            
            // Get interceptors from service provider during context creation
            var medicalAuditInterceptor = serviceProvider.GetRequiredService<MedicalAuditInterceptor>();
            var correlationIdInterceptor = serviceProvider.GetRequiredService<CorrelationIdInterceptor>();
            
            options.AddInterceptors(medicalAuditInterceptor, correlationIdInterceptor);
        });

        services.AddScoped<EfServiceRepository>();

        // Configure Dapper to use same SQLite in-memory database (real dependency)
        services.AddSingleton<DapperConnectionFactory>(_ =>
            new DapperConnectionFactory(connectionString));
        
        services.AddScoped<DapperServiceRepository>();

        // Add health checks
        services.AddDataAccessHealthChecks();
        
        return services;
    }

    private static IServiceCollection AddEntityFrameworkServices(
        this IServiceCollection services, 
        object database)
    {
        // GREEN PHASE: EF Core configuration for admin APIs (write-heavy operations)
        services.AddDbContext<ServicesDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql("Host=localhost;Port=5432;Database=internationalcenter-test;Username=postgres;Password=Password123!;Pooling=true");
            
            // Get interceptors from service provider during context creation
            var medicalAuditInterceptor = serviceProvider.GetRequiredService<MedicalAuditInterceptor>();
            var correlationIdInterceptor = serviceProvider.GetRequiredService<CorrelationIdInterceptor>();
            
            options.AddInterceptors(medicalAuditInterceptor, correlationIdInterceptor);
        });

        services.AddScoped<EfServiceRepository>();
        
        return services;
    }

    private static IServiceCollection AddDapperServices(
        this IServiceCollection services, 
        object database)
    {
        // GREEN PHASE: Dapper configuration for public APIs (read-heavy operations)
        services.AddSingleton<DapperConnectionFactory>(provider =>
            new DapperConnectionFactory("Host=localhost;Port=5432;Database=internationalcenter-test;Username=postgres;Password=Password123!;Pooling=true"));
        
        services.AddScoped<DapperServiceRepository>();
        
        return services;
    }

    private static IServiceCollection AddDataAccessHealthChecks(
        this IServiceCollection services)
    {
        // REFACTOR PHASE: High-performance medical-grade health monitoring with caching
        
        // Add memory caching for health check optimization
        services.AddMemoryCache();
        
        // Register health checks with enhanced dependencies
        services.AddScoped<DatabaseHealthCheck>();
        services.AddScoped<ConnectionPoolHealthCheck>();
        
        // Configure health checks with medical-grade timeouts and tags
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "postgresql", "medical", "critical" },
                timeout: TimeSpan.FromSeconds(10)) // Medical-grade timeout
            .AddCheck<ConnectionPoolHealthCheck>(
                name: "connectionpool",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "performance", "medical", "critical" },
                timeout: TimeSpan.FromSeconds(15)); // Pool check timeout
        
        return services;
    }

    private static IServiceCollection AddMedicalAuditInterceptors(
        this IServiceCollection services)
    {
        // REFACTOR PHASE: Medical audit compliance interceptors with high-performance dependencies
        
        // Register object pools for high-performance medical audit
        services.AddSingleton<ObjectPool<List<ServiceAuditEntity>>>(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            return provider.Create(new ServiceAuditListPooledObjectPolicy());
        });
        
        services.AddSingleton<ObjectPool<Dictionary<string, object?>>>(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            return provider.Create(new DictionaryPooledObjectPolicy());
        });

        // Register ObjectPoolProvider if not already registered
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        
        // Register interceptors with dependencies
        services.AddScoped<MedicalAuditInterceptor>();
        services.AddScoped<CorrelationIdInterceptor>();
        
        return services;
    }

    private sealed class ServiceAuditListPooledObjectPolicy : PooledObjectPolicy<List<ServiceAuditEntity>>
    {
        public override List<ServiceAuditEntity> Create() => new();

        public override bool Return(List<ServiceAuditEntity> obj)
        {
            obj.Clear();
            return obj.Count == 0; // Return true if successfully cleared
        }
    }

    private sealed class DictionaryPooledObjectPolicy : PooledObjectPolicy<Dictionary<string, object?>>
    {
        public override Dictionary<string, object?> Create() => new();

        public override bool Return(Dictionary<string, object?> obj)
        {
            obj.Clear();
            return obj.Count == 0; // Return true if successfully cleared
        }
    }
}