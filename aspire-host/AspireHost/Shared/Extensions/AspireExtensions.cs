using Aspire.Hosting;
using AspireHost.Features.ServiceDiscovery;
using AspireHost.Features.ResourceOrchestration;
using AspireHost.Features.EnvironmentManagement;
using AspireHost.Features.HealthOrchestration;
using AspireHost.Shared.Configuration;

namespace AspireHost.Shared.Extensions;

public static class AspireExtensions
{
    public static IDistributedApplicationBuilder AddInternationalCenterServices(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Medical-grade service configuration with enhanced reliability
        
        // Add Services Domain with enhanced configuration for medical-grade reliability
        var servicesDomain = builder.AddContainer("services-domain", "services-domain")
            .WithHttpEndpoint(port: 5000, name: "services-api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false")
            .WithEnvironment("ASPNETCORE_URLS", "http://+:5000")
            // Medical-grade environment configuration
            .WithEnvironment("LOGGING__LOGLEVEL__DEFAULT", "Information")
            .WithEnvironment("LOGGING__LOGLEVEL__SYSTEM", "Warning")
            .WithEnvironment("LOGGING__LOGLEVEL__MICROSOFT", "Warning");
        
        // Add API Gateway with enhanced configuration for medical-grade reliability
        var apiGateway = builder.AddContainer("api-gateway", "api-gateway")
            .WithHttpEndpoint(port: 5001, name: "gateway-api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false")
            .WithEnvironment("ASPNETCORE_URLS", "http://+:5001")
            // Medical-grade environment configuration
            .WithEnvironment("LOGGING__LOGLEVEL__DEFAULT", "Information")
            .WithEnvironment("LOGGING__LOGLEVEL__SYSTEM", "Warning")
            .WithEnvironment("LOGGING__LOGLEVEL__MICROSOFT", "Warning");
        
        return builder;
    }

    public static IDistributedApplicationBuilder AddInfrastructureResources(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Medical-grade infrastructure with enhanced reliability and compliance
        
        // Add PostgreSQL database with medical-grade configuration
        var postgres = builder.AddPostgres("postgres")
            .WithEnvironment("POSTGRES_DB", "internationalcenter")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "Password123!")
            // Medical-grade PostgreSQL performance and audit settings
            .WithEnvironment("POSTGRES_SHARED_PRELOAD_LIBRARIES", "pg_stat_statements")
            .WithEnvironment("POSTGRES_MAX_CONNECTIONS", "100")
            .WithEnvironment("POSTGRES_SHARED_BUFFERS", "256MB")
            .WithEnvironment("POSTGRES_EFFECTIVE_CACHE_SIZE", "1GB")
            .WithEnvironment("POSTGRES_WORK_MEM", "4MB")
            .WithEnvironment("POSTGRES_MAINTENANCE_WORK_MEM", "64MB")
            .WithEnvironment("POSTGRES_LOG_STATEMENT", "all")
            .WithEnvironment("POSTGRES_LOG_DURATION", "on")
            .WithEnvironment("POSTGRES_LOG_MIN_DURATION_STATEMENT", "100")
            .WithEnvironment("POSTGRES_ARCHIVE_MODE", "on")
            .WithEnvironment("POSTGRES_WAL_LEVEL", "replica")
            .WithEnvironment("POSTGRES_MAX_WAL_SENDERS", "3")
            .WithDataVolume()
            .WithPgAdmin();
        
        var postgresDb = postgres.AddDatabase("internationalcenter");
        
        // Add Redis with medical-grade configuration for distributed caching
        var redis = builder.AddRedis("redis")
            .WithEnvironment("REDIS_MAXMEMORY", "256mb")
            .WithEnvironment("REDIS_MAXMEMORY_POLICY", "allkeys-lru")
            .WithEnvironment("REDIS_SAVE", "60 1000")
            .WithEnvironment("REDIS_APPENDONLY", "yes")
            .WithEnvironment("REDIS_APPENDFSYNC", "everysec")
            .WithRedisCommander();
        
        // Add RabbitMQ with medical-grade configuration for messaging reliability
        var rabbitmq = builder.AddRabbitMQ("rabbitmq")
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "admin")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "Password123!")
            .WithEnvironment("RABBITMQ_VM_MEMORY_HIGH_WATERMARK", "0.8")
            .WithEnvironment("RABBITMQ_DISK_FREE_LIMIT", "1GB")
            .WithEnvironment("RABBITMQ_LOG_LEVEL", "info")
            .WithEnvironment("RABBITMQ_CLUSTER_NAME", "international-center-cluster")
            .WithManagementPlugin();
        
        return builder;
    }

    public static IDistributedApplicationBuilder ConfigureEnvironment(this IDistributedApplicationBuilder builder)
    {
        // Configure for Development environment with session containers
        builder.Configuration["Environment"] = "Development";
        builder.Configuration["SessionContainers"] = "true";
        builder.Configuration["PersistentDataVolumes"] = "true";
        builder.Configuration["MedicalGradeCompliance"] = "true";
        
        // Set up connection strings for service discovery
        builder.Configuration["ConnectionStrings:postgres"] = "Host=localhost;Port=5432;Database=internationalcenter;Username=postgres;Password=Password123!;Pooling=true";
        builder.Configuration["ConnectionStrings:redis"] = "localhost:6379";
        builder.Configuration["ConnectionStrings:rabbitmq"] = "amqp://guest:guest@localhost:5672";
        builder.Configuration["ConnectionStrings:services-domain"] = "http://localhost:5000";
        builder.Configuration["ConnectionStrings:api-gateway"] = "http://localhost:5001";
        builder.Configuration["ConnectionStrings:audit"] = "Host=localhost;Port=5432;Database=internationalcenter;Username=postgres;Password=Password123!;Pooling=true";
        
        return builder;
    }

    public static IDistributedApplicationBuilder AddHealthOrchestration(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Medical-grade centralized health orchestration placeholder
        // Health checks will be configured in individual services for medical-grade monitoring
        // This provides foundation for centralized health coordination across distributed services
        
        return builder;
    }

    public static IDistributedApplicationBuilder AddObservability(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Medical-grade observability with comprehensive monitoring
        
        // Add Prometheus with medical-grade configuration for metrics collection
        var prometheus = builder.AddContainer("prometheus", "prom/prometheus:latest")
            .WithBindMount("./prometheus.yml", "/etc/prometheus/prometheus.yml")
            .WithHttpEndpoint(port: 9090, name: "prometheus-ui")
            .WithEnvironment("PROMETHEUS_STORAGE_TSDB_RETENTION_TIME", "30d")
            .WithEnvironment("PROMETHEUS_STORAGE_TSDB_RETENTION_SIZE", "10GB")
            .WithEnvironment("PROMETHEUS_WEB_ENABLE_ADMIN_API", "true")
            .WithEnvironment("PROMETHEUS_WEB_ENABLE_LIFECYCLE", "true");
        
        // Add Grafana with medical-grade configuration for dashboards
        var grafana = builder.AddContainer("grafana", "grafana/grafana:latest")
            .WithHttpEndpoint(port: 3000, name: "grafana-ui")
            .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "Password123!")
            .WithEnvironment("GF_SECURITY_SECRET_KEY", "medical-grade-secret-key-change-in-production")
            .WithEnvironment("GF_SECURITY_DISABLE_GRAVATAR", "true")
            .WithEnvironment("GF_USERS_ALLOW_SIGN_UP", "false")
            .WithEnvironment("GF_USERS_ALLOW_ORG_CREATE", "false")
            .WithEnvironment("GF_LOG_MODE", "console,file")
            .WithEnvironment("GF_LOG_LEVEL", "info")
            .WithEnvironment("GF_ANALYTICS_REPORTING_ENABLED", "false")
            .WithEnvironment("GF_ANALYTICS_CHECK_FOR_UPDATES", "false")
            .WithEnvironment("GF_DATABASE_TYPE", "sqlite3")
            .WithEnvironment("GF_DATABASE_PATH", "/var/lib/grafana/grafana.db")
            .WithEnvironment("GF_PATHS_DATA", "/var/lib/grafana")
            .WithEnvironment("GF_PATHS_LOGS", "/var/log/grafana")
            .WithEnvironment("GF_SESSION_PROVIDER", "file")
            .WithEnvironment("GF_SESSION_COOKIE_SECURE", "false")
            .WithEnvironment("GF_SESSION_COOKIE_SAMESITE", "strict");
        
        return builder;
    }
}