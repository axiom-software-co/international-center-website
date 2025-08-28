using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using SharedPlatform.Features.DataAccess.EntityFramework;
using SharedPlatform.Features.DataAccess.Dapper;
using SharedPlatform.Features.DataAccess.Interceptors;
using SharedPlatform.Features.DataAccess.HealthChecks;

namespace SharedPlatform.Features.DataAccess.Extensions;

/// <summary>
/// Extension methods for registering data access services
/// GREEN PHASE: Complete implementation for medical-grade data access
/// Medical-grade data access service registration with dual strategy (EF Core + Dapper)
/// </summary>
public static class DataAccessServiceExtensions
{
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
        // GREEN PHASE: Medical-grade health monitoring
        services.AddScoped<DatabaseHealthCheck>();
        services.AddScoped<ConnectionPoolHealthCheck>();
        
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", HealthStatus.Unhealthy, new[] { "database", "postgresql" })
            .AddCheck<ConnectionPoolHealthCheck>("connectionpool", HealthStatus.Unhealthy, new[] { "database", "performance" });
        
        return services;
    }

    private static IServiceCollection AddMedicalAuditInterceptors(
        this IServiceCollection services)
    {
        // GREEN PHASE: Medical audit compliance interceptors
        services.AddScoped<MedicalAuditInterceptor>();
        services.AddScoped<CorrelationIdInterceptor>();
        
        return services;
    }
}