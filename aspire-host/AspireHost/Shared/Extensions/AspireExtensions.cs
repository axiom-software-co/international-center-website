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
        // Add Services Domain with database connection
        var servicesDomain = builder.AddContainer("services-domain", "services-domain")
            .WithHttpEndpoint(port: 5000);
        
        // Add API Gateway with YARP routing  
        var apiGateway = builder.AddContainer("api-gateway", "api-gateway")
            .WithHttpEndpoint(port: 5001);
        
        return builder;
    }

    public static IDistributedApplicationBuilder AddInfrastructureResources(this IDistributedApplicationBuilder builder)
    {
        // Add PostgreSQL database for medical-grade data persistence
        var postgres = builder.AddPostgres("postgres")
            .WithEnvironment("POSTGRES_DB", "internationalcenter")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "Password123!")
            .WithPgAdmin();
        
        var postgresDb = postgres.AddDatabase("internationalcenter");
        
        // Add Redis for distributed caching
        var redis = builder.AddRedis("redis")
            .WithRedisCommander();
        
        // Add RabbitMQ for messaging
        var rabbitmq = builder.AddRabbitMQ("rabbitmq")
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
        // Health checks will be configured in individual services
        // This is a placeholder for centralized health orchestration
        return builder;
    }

    public static IDistributedApplicationBuilder AddObservability(this IDistributedApplicationBuilder builder)
    {
        // Add Prometheus for metrics collection
        var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
            .WithBindMount("./prometheus.yml", "/etc/prometheus/prometheus.yml")
            .WithHttpEndpoint(port: 9090);
        
        // Add Grafana for metrics visualization
        var grafana = builder.AddContainer("grafana", "grafana/grafana")
            .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
            .WithHttpEndpoint(port: 3000);
        
        return builder;
    }
}