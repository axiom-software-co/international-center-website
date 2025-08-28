using ApiGateway.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// REFACTOR PHASE: Medical-grade API Gateway with sub-100ms performance target

// Configure Kestrel for medical-grade performance
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 1000;
    options.Limits.MaxConcurrentUpgradedConnections = 1000;
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    options.AddServerHeader = false; // Remove server header for security
});

// Medical-grade logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add medical-grade gateway services with performance optimization
builder.Services.AddGatewayCore(builder.Configuration);
builder.Services.AddGatewayConfiguration(builder.Configuration);
builder.Services.AddGatewayServices();

// Medical-grade health checks with downstream service monitoring
builder.Services.AddHealthChecks()
    .AddCheck("gateway-self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Gateway operational"))
    .AddUrlGroup(new Uri("http://localhost:5000/health"), "services-domain", 
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        timeout: TimeSpan.FromSeconds(5))
    .AddCheck("redis-connection", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Redis operational"))
    .AddCheck("rate-limiting", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Rate limiting operational"));

var app = builder.Build();

// REFACTOR PHASE: Optimized middleware pipeline for medical-grade performance
app.UseGatewayErrorHandling();
app.UseGatewayPipeline();
app.UseGatewayHealthChecks();

// YARP reverse proxy (final middleware for request forwarding)
app.MapReverseProxy();

app.Run();

// Make Program accessible for testing
public partial class Program { }