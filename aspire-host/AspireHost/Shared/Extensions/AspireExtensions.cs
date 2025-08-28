using Aspire.Hosting;
using AspireHost.Features.ServiceDiscovery;
using AspireHost.Features.ResourceOrchestration;
using AspireHost.Features.EnvironmentManagement;
using AspireHost.Features.HealthOrchestration;
using AspireHost.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace AspireHost.Shared.Extensions;

/// <summary>
/// High-performance logging for AspireHost resource orchestration operations
/// Uses LoggerMessage delegates for optimal production performance
/// </summary>
internal static partial class AspireLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Configuring international center services for environment: {Environment}")]
    public static partial void ConfiguringInternationalCenterServices(ILogger logger, string environment);
    
    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Adding services domain container with endpoint: {Endpoint}")]
    public static partial void AddingServicesDomainContainer(ILogger logger, string endpoint);
    
    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Adding API gateway container with endpoint: {Endpoint}")]
    public static partial void AddingApiGatewayContainer(ILogger logger, string endpoint);
    
    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Configuring infrastructure resources for environment: {Environment}")]
    public static partial void ConfiguringInfrastructureResources(ILogger logger, string environment);
    
    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Adding PostgreSQL database with performance settings")]
    public static partial void AddingPostgreSqlDatabase(ILogger logger);
    
    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Adding Redis cache with memory limit: {MemoryLimit}")]
    public static partial void AddingRedisCache(ILogger logger, string memoryLimit);
    
    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Adding RabbitMQ messaging with cluster: {ClusterName}")]
    public static partial void AddingRabbitMqMessaging(ILogger logger, string clusterName);
    
    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Configuring content management resources with Azurite emulator")]
    public static partial void ConfiguringContentManagementResources(ILogger logger);
    
    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = "Adding Azure Storage emulator with blob port: {BlobPort}")]
    public static partial void AddingAzureStorageEmulator(ILogger logger, int blobPort);
    
    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, Message = "Adding CDN simulation container with endpoint: {Endpoint}")]
    public static partial void AddingCdnSimulationContainer(ILogger logger, string endpoint);
    
    [LoggerMessage(EventId = 4001, Level = LogLevel.Information, Message = "Configuring health orchestration with {HealthCheckCount} health checks")]
    public static partial void ConfiguringHealthOrchestration(ILogger logger, int healthCheckCount);
    
    [LoggerMessage(EventId = 4002, Level = LogLevel.Debug, Message = "Adding health check for service: {ServiceName} with tags: {Tags}")]
    public static partial void AddingHealthCheck(ILogger logger, string serviceName, string tags);
    
    [LoggerMessage(EventId = 4003, Level = LogLevel.Warning, Message = "Skipping health check for {ServiceName} - connection string not available")]
    public static partial void SkippingHealthCheck(ILogger logger, string serviceName);
    
    [LoggerMessage(EventId = 5001, Level = LogLevel.Information, Message = "Configuring environment settings - PersistentVolumes: {PersistentVolumes}, Compliance: {Compliance}")]
    public static partial void ConfiguringEnvironmentSettings(ILogger logger, bool persistentVolumes, bool compliance);
    
    [LoggerMessage(EventId = 6001, Level = LogLevel.Information, Message = "Configuring observability with Prometheus and Grafana containers")]
    public static partial void ConfiguringObservability(ILogger logger);
    
    [LoggerMessage(EventId = 9001, Level = LogLevel.Error, Message = "Failed to configure resource: {ResourceName}")]
    public static partial void ResourceConfigurationFailed(ILogger logger, string resourceName, Exception exception);
    
    [LoggerMessage(EventId = 9002, Level = LogLevel.Critical, Message = "Critical failure in AspireHost resource orchestration")]
    public static partial void CriticalOrchestrationFailure(ILogger logger, Exception exception);
}

public static class AspireExtensions
{
    public static IDistributedApplicationBuilder AddInternationalCenterServices(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Service configuration with enhanced reliability and production logging
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<object>>();
        var environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        
        try
        {
            AspireLog.ConfiguringInternationalCenterServices(logger, environment);
            
            // Add Services Domain with enhanced configuration for reliability
            AspireLog.AddingServicesDomainContainer(logger, "http://+:5000");
            var servicesDomain = builder.AddContainer("services-domain", "services-domain")
                .WithHttpEndpoint(port: 5000, name: "services-api")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment)
                .WithEnvironment("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false")
                .WithEnvironment("ASPNETCORE_URLS", "http://+:5000")
                // Production environment configuration
                .WithEnvironment("LOGGING__LOGLEVEL__DEFAULT", "Information")
                .WithEnvironment("LOGGING__LOGLEVEL__SYSTEM", "Warning")
                .WithEnvironment("LOGGING__LOGLEVEL__MICROSOFT", "Warning");
            
            // Add API Gateway with enhanced configuration for reliability
            AspireLog.AddingApiGatewayContainer(logger, "http://+:5001");
            var apiGateway = builder.AddContainer("api-gateway", "api-gateway")
                .WithHttpEndpoint(port: 5001, name: "gateway-api")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment)
                .WithEnvironment("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false")
                .WithEnvironment("ASPNETCORE_URLS", "http://+:5001")
                // Production environment configuration
                .WithEnvironment("LOGGING__LOGLEVEL__DEFAULT", "Information")
                .WithEnvironment("LOGGING__LOGLEVEL__SYSTEM", "Warning")
                .WithEnvironment("LOGGING__LOGLEVEL__MICROSOFT", "Warning");
            
            return builder;
        }
        catch (Exception ex)
        {
            AspireLog.CriticalOrchestrationFailure(logger, ex);
            throw;
        }
    }

    public static IDistributedApplicationBuilder AddInfrastructureResources(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Infrastructure with enhanced reliability, compliance and production logging
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<object>>();
        var environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        
        try
        {
            AspireLog.ConfiguringInfrastructureResources(logger, environment);
            
            // Add PostgreSQL database with production configuration
            AspireLog.AddingPostgreSqlDatabase(logger);
            var postgres = builder.AddPostgres("postgres")
                .WithEnvironment("POSTGRES_DB", "internationalcenter")
                .WithEnvironment("POSTGRES_USER", "postgres")
                .WithEnvironment("POSTGRES_PASSWORD", "Password123!")
                // Production PostgreSQL performance and audit settings
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
            
            // Add Redis with production configuration for distributed caching
            AspireLog.AddingRedisCache(logger, "256mb");
            var redis = builder.AddRedis("redis")
                .WithEnvironment("REDIS_MAXMEMORY", "256mb")
                .WithEnvironment("REDIS_MAXMEMORY_POLICY", "allkeys-lru")
                .WithEnvironment("REDIS_SAVE", "60 1000")
                .WithEnvironment("REDIS_APPENDONLY", "yes")
                .WithEnvironment("REDIS_APPENDFSYNC", "everysec")
                .WithRedisCommander();
            
            // Add RabbitMQ with production configuration for messaging reliability
            AspireLog.AddingRabbitMqMessaging(logger, "international-center-cluster");
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
        catch (Exception ex)
        {
            AspireLog.CriticalOrchestrationFailure(logger, ex);
            throw;
        }
    }

    public static IDistributedApplicationBuilder ConfigureEnvironment(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Environment configuration with production logging and compliance settings
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<object>>();
        
        try
        {
            // Configure for Development environment with session containers
            var environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            builder.Configuration["Environment"] = environment;
            builder.Configuration["SessionContainers"] = "true";
            builder.Configuration["PersistentDataVolumes"] = "true";
            builder.Configuration["MedicalGradeCompliance"] = "true";
            
            AspireLog.ConfiguringEnvironmentSettings(logger, persistentVolumes: true, compliance: true);
            
            // Set up connection strings for service discovery
            builder.Configuration["ConnectionStrings:postgres"] = "Host=localhost;Port=5432;Database=internationalcenter;Username=postgres;Password=Password123!;Pooling=true";
            builder.Configuration["ConnectionStrings:redis"] = "localhost:6379";
            builder.Configuration["ConnectionStrings:rabbitmq"] = "amqp://guest:guest@localhost:5672";
            builder.Configuration["ConnectionStrings:services-domain"] = "http://localhost:5000";
            builder.Configuration["ConnectionStrings:api-gateway"] = "http://localhost:5001";
            builder.Configuration["ConnectionStrings:audit"] = "Host=localhost;Port=5432;Database=internationalcenter;Username=postgres;Password=Password123!;Pooling=true";
            
            return builder;
        }
        catch (Exception ex)
        {
            AspireLog.ResourceConfigurationFailed(logger, "Environment", ex);
            throw;
        }
    }

    public static IDistributedApplicationBuilder AddHealthOrchestration(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Centralized health orchestration with comprehensive monitoring and production logging
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<object>>();
        var healthCheckCount = 0;
        
        try
        {
            var healthChecksBuilder = builder.Services.AddHealthChecks();
            
            // Core infrastructure health checks (only add if connection strings are available)
            var postgresConnectionString = builder.Configuration.GetConnectionString("postgres");
            if (!string.IsNullOrEmpty(postgresConnectionString))
            {
                healthChecksBuilder.AddNpgSql(postgresConnectionString, name: "postgres", tags: ["database", "infrastructure"]);
                AspireLog.AddingHealthCheck(logger, "postgres", "database,infrastructure");
                healthCheckCount++;
            }
            else
            {
                AspireLog.SkippingHealthCheck(logger, "postgres");
            }
            
            var redisConnectionString = builder.Configuration.GetConnectionString("redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                healthChecksBuilder.AddRedis(redisConnectionString, name: "redis", tags: ["cache", "infrastructure"]);
                AspireLog.AddingHealthCheck(logger, "redis", "cache,infrastructure");
                healthCheckCount++;
            }
            else
            {
                AspireLog.SkippingHealthCheck(logger, "redis");
            }
            
            var rabbitmqConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
            if (!string.IsNullOrEmpty(rabbitmqConnectionString))
            {
                healthChecksBuilder.AddRabbitMQ(rabbitmqConnectionString, name: "rabbitmq", tags: ["messaging", "infrastructure"]);
                AspireLog.AddingHealthCheck(logger, "rabbitmq", "messaging,infrastructure");
                healthCheckCount++;
            }
            else
            {
                AspireLog.SkippingHealthCheck(logger, "rabbitmq");
            }
            
            // ContentManagement infrastructure health checks (only add if connection strings are available)
            var storageConnectionString = builder.Configuration.GetConnectionString("storage");
            if (!string.IsNullOrEmpty(storageConnectionString))
            {
                healthChecksBuilder.AddAzureBlobStorage(_ => new BlobServiceClient(storageConnectionString), name: "azure-storage", tags: ["storage", "content-management"]);
                AspireLog.AddingHealthCheck(logger, "azure-storage", "storage,content-management");
                healthCheckCount++;
            }
            else
            {
                AspireLog.SkippingHealthCheck(logger, "azure-storage");
            }
            
            var cdnSimulationUrl = builder.Configuration.GetConnectionString("cdn-simulation");
            if (!string.IsNullOrEmpty(cdnSimulationUrl) && Uri.TryCreate(cdnSimulationUrl, UriKind.Absolute, out var cdnUri))
            {
                healthChecksBuilder.AddUrlGroup(cdnUri, name: "cdn-simulation", tags: ["cdn", "content-management"]);
                AspireLog.AddingHealthCheck(logger, "cdn-simulation", "cdn,content-management");
                healthCheckCount++;
            }
            else
            {
                AspireLog.SkippingHealthCheck(logger, "cdn-simulation");
            }
            
            // Service health checks (only add if connection strings are available)
            var servicesDomainUrl = builder.Configuration.GetConnectionString("services-domain");
            if (!string.IsNullOrEmpty(servicesDomainUrl) && Uri.TryCreate(servicesDomainUrl, UriKind.Absolute, out var servicesDomainUri))
            {
                healthChecksBuilder.AddUrlGroup(servicesDomainUri, name: "services-domain", tags: ["service", "api"]);
                AspireLog.AddingHealthCheck(logger, "services-domain", "service,api");
                healthCheckCount++;
            }
            else
            {
                AspireLog.SkippingHealthCheck(logger, "services-domain");
            }
            
            var apiGatewayUrl = builder.Configuration.GetConnectionString("api-gateway");
            if (!string.IsNullOrEmpty(apiGatewayUrl) && Uri.TryCreate(apiGatewayUrl, UriKind.Absolute, out var apiGatewayUri))
            {
                healthChecksBuilder.AddUrlGroup(apiGatewayUri, name: "api-gateway", tags: ["gateway", "api"]);
                AspireLog.AddingHealthCheck(logger, "api-gateway", "gateway,api");
                healthCheckCount++;
            }
            else
            {
                AspireLog.SkippingHealthCheck(logger, "api-gateway");
            }
            
            AspireLog.ConfiguringHealthOrchestration(logger, healthCheckCount);
            
            // Configure health check UI endpoint
            builder.Services.Configure<HealthCheckOptions>(options =>
            {
                options.AllowCachingResponses = false;
                options.ResponseWriter = async (context, report) =>
                {
                    var result = new
                    {
                        status = report.Status.ToString(),
                        totalDuration = report.TotalDuration.TotalMilliseconds,
                        entries = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            duration = e.Value.Duration.TotalMilliseconds,
                            description = e.Value.Description,
                            tags = e.Value.Tags
                        })
                    };
                    
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
                };
            });
            
            return builder;
        }
        catch (Exception ex)
        {
            AspireLog.ResourceConfigurationFailed(logger, "HealthOrchestration", ex);
            throw;
        }
    }

    public static IDistributedApplicationBuilder AddContentManagementResources(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: ContentManagement infrastructure resource orchestration with production logging
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<object>>();
        
        try
        {
            AspireLog.ConfiguringContentManagementResources(logger);
            
            // Add Azure Storage with Azurite emulator for local development (Microsoft-recommended approach)
            AspireLog.AddingAzureStorageEmulator(logger, 10000);
            var storage = builder.AddAzureStorage("storage")
                .RunAsEmulator(emulator => emulator
                    .WithBlobPort(10000)
                    .WithQueuePort(10001)
                    .WithTablePort(10002)
                    .WithDataVolume("azurite-data"));
            
            // Add blob storage specifically for content management
            var blobs = storage.AddBlobs("blobs");
            
            // Add nginx container for CDN simulation
            AspireLog.AddingCdnSimulationContainer(logger, "http://localhost:8080");
            var cdnSimulation = builder.AddContainer("cdn-simulation", "nginx:alpine")
                .WithHttpEndpoint(port: 8080, name: "cdn-api")
                .WithEnvironment("NGINX_HOST", "localhost")
                .WithEnvironment("NGINX_PORT", "8080")
                .WithBindMount("./nginx-cdn.conf", "/etc/nginx/nginx.conf");
            
            // Configure environment-specific settings
            builder.Configuration["ContentStorage:UseEmulator"] = "true";
            builder.Configuration["ContentStorage:BlobStorageUrl"] = "http://127.0.0.1:10000/devstoreaccount1";
            builder.Configuration["CDN:BaseUrl"] = "http://localhost:8080";
            builder.Configuration["CDN:UrlPattern"] = "/services/content/{service-id}/{content-hash}.html";
            builder.Configuration["CDN:UseSimulation"] = "true";
            
            // Configure health check settings
            builder.Configuration["HealthChecks:ContentManagement"] = "storage,cdn-simulation";
            
            // Configure resource startup order
            builder.Configuration["ResourceStartup:Postgres:Order"] = "1";
            builder.Configuration["ResourceStartup:Storage:Order"] = "2";
            builder.Configuration["ResourceStartup:CdnSimulation:Order"] = "3";
            
            // Configure connection strings for service discovery (Microsoft-recommended patterns)
            builder.Configuration["ConnectionStrings:storage"] = "UseDevelopmentStorage=true";
            builder.Configuration["ConnectionStrings:blobs"] = "UseDevelopmentStorage=true";
            builder.Configuration["ConnectionStrings:blob-storage"] = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
            builder.Configuration["ConnectionStrings:cdn-simulation"] = "http://localhost:8080";
            builder.Configuration["ConnectionStrings:content-management"] = "http://localhost:8080/content-management";
            
            return builder;
        }
        catch (Exception ex)
        {
            AspireLog.ResourceConfigurationFailed(logger, "ContentManagement", ex);
            throw;
        }
    }

    public static IDistributedApplicationBuilder AddObservability(this IDistributedApplicationBuilder builder)
    {
        // REFACTOR PHASE: Observability with comprehensive monitoring and production logging
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<object>>();
        
        try
        {
            AspireLog.ConfiguringObservability(logger);
            
            // Add Prometheus with production configuration for metrics collection
            var prometheus = builder.AddContainer("prometheus", "prom/prometheus:latest")
                .WithBindMount("./prometheus.yml", "/etc/prometheus/prometheus.yml")
                .WithHttpEndpoint(port: 9090, name: "prometheus-ui")
                .WithEnvironment("PROMETHEUS_STORAGE_TSDB_RETENTION_TIME", "30d")
                .WithEnvironment("PROMETHEUS_STORAGE_TSDB_RETENTION_SIZE", "10GB")
                .WithEnvironment("PROMETHEUS_WEB_ENABLE_ADMIN_API", "true")
                .WithEnvironment("PROMETHEUS_WEB_ENABLE_LIFECYCLE", "true");
            
            // Add Grafana with production configuration for dashboards
            var grafana = builder.AddContainer("grafana", "grafana/grafana:latest")
                .WithHttpEndpoint(port: 3000, name: "grafana-ui")
                .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "Password123!")
                .WithEnvironment("GF_SECURITY_SECRET_KEY", "production-secret-key-change-in-production")
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
        catch (Exception ex)
        {
            AspireLog.ResourceConfigurationFailed(logger, "Observability", ex);
            throw;
        }
    }
}