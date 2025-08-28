using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ApiGateway.Shared.Abstractions;
using ApiGateway.Features.Authentication;
using ApiGateway.Features.Authorization;
using ApiGateway.Features.RateLimiting;
using ApiGateway.Features.Security;
using ApiGateway.Features.ErrorHandling;
using ApiGateway.Features.Observability;
using ApiGateway.Features.HealthChecks;
using ApiGateway.Features.Cors;

namespace ApiGateway.Shared.Extensions;

public static class WebApplicationExtensions
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
    public static WebApplication UseGatewayPipeline(this WebApplication app)
    {
        // REFACTOR PHASE: Medical-grade optimized middleware pipeline for sub-100ms response times
        
        // 1. Response compression (earliest for maximum efficiency)
        app.UseResponseCompression();
        
        // 2. Response caching (before authentication for public content)
        app.UseResponseCaching();
        
        // 3. Security headers (critical security layer)
        app.UseMiddleware<SecurityHeadersMiddleware>();
        
        // 4. Request validation and anti-fraud protection
        app.UseMiddleware<RequestValidationMiddleware>();
        
        // 5. CORS (before authentication)
        app.UseMiddleware<CorsMiddleware>();
        
        // 6. Error handling (early to catch all errors)
        app.UseMiddleware<ErrorHandlingMiddleware>();
        
        // 7. Observability and request logging (for audit compliance)
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<MetricsCollectionMiddleware>();
        app.UseMiddleware<TracingMiddleware>();
        
        // 8. Rate limiting (after logging for audit compliance)
        app.UseMiddleware<RateLimitingMiddleware>();
        
        // 9. Authentication (after rate limiting)
        app.UseMiddleware<AuthenticationMiddleware>();
        
        // 10. Authorization (final security check)
        app.UseMiddleware<AuthorizationMiddleware>();
        
        // 11. Response security middleware (final response processing)
        app.UseMiddleware<ResponseSecurityMiddleware>();
        
        return app;
    }
    
    public static WebApplication UseGatewayMiddleware(this WebApplication app)
    {
        var middlewareProvider = app.Services.GetService<IMiddlewareProvider>();
        return middlewareProvider?.ConfigureMiddleware(app) as WebApplication ?? app;
    }
    
    public static WebApplication UseGatewayHealthChecks(this WebApplication app)
    {
        // REFACTOR PHASE: Medical-grade health checks with performance optimization
        
        // Map optimized health check endpoints
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    duration = report.TotalDuration,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration,
                        description = e.Value.Description
                    })
                }, JsonOptions);
                await context.Response.WriteAsync(result).ConfigureAwait(false);
            }
        });
        
        // Quick readiness check for load balancers (ultra-fast response)
        app.MapGet("/ready", () => Results.Ok(new { status = "ready", timestamp = DateTime.UtcNow }));
        
        // Liveness check (minimal overhead)
        app.MapGet("/live", () => Results.Ok(new { status = "alive", timestamp = DateTime.UtcNow }));
        
        return app;
    }
    
    public static WebApplication UseGatewayErrorHandling(this WebApplication app)
    {
        // REFACTOR PHASE: Medical-grade error handling with performance optimization
        
        // Global exception handler for unhandled exceptions
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                
                var errorHandler = context.RequestServices.GetRequiredService<GatewayErrorHandler>();
                var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                
                if (exception != null)
                {
                    await errorHandler.HandleErrorAsync(context, exception).ConfigureAwait(false);
                }
            });
        });
        
        return app;
    }
}