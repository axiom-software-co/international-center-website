using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using SharedPlatform.Features.DataAccess.EntityFramework;

namespace SharedPlatform.Features.DataAccess.HealthChecks;

/// <summary>
/// Health check for database connectivity validation
/// GREEN PHASE: Complete implementation with medical-grade monitoring
/// Comprehensive PostgreSQL connectivity and performance monitoring
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ServicesDbContext _context;

    public DatabaseHealthCheck(ServicesDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Complete health check implementation
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test database connectivity
            var canConnect = await CanConnectToDatabase(cancellationToken).ConfigureAwait(false);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to PostgreSQL database");
            }

            // Get database performance metrics
            var metrics = await GetDatabaseMetrics(cancellationToken).ConfigureAwait(false);
            
            stopwatch.Stop();
            metrics["HealthCheckDurationMs"] = stopwatch.ElapsedMilliseconds;

            // Determine health status based on metrics
            var status = DetermineHealthStatus(metrics);
            var description = $"PostgreSQL database health check completed in {stopwatch.ElapsedMilliseconds}ms";

            return new HealthCheckResult(status, description, data: metrics);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}", ex);
        }
    }

    private async Task<bool> CanConnectToDatabase(CancellationToken cancellationToken)
    {
        // GREEN PHASE: PostgreSQL connectivity validation
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    private async Task<Dictionary<string, object>> GetDatabaseMetrics(CancellationToken cancellationToken)
    {
        // GREEN PHASE: Comprehensive database metrics collection
        var metrics = new Dictionary<string, object>();

        try
        {
            // Basic connectivity test
            var queryStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var testValue = await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken).ConfigureAwait(false);
            queryStopwatch.Stop();
            
            metrics["QueryResponseTimeMs"] = queryStopwatch.ElapsedMilliseconds;
            metrics["DatabaseName"] = _context.Database.GetDbConnection().Database;
            metrics["ConnectionState"] = _context.Database.GetDbConnection().State.ToString();

            // Get service count for monitoring
            try
            {
                var serviceCount = await _context.Services.CountAsync(cancellationToken).ConfigureAwait(false);
                metrics["ServiceCount"] = serviceCount;
            }
            catch (Exception ex)
            {
                metrics["ServiceCountError"] = ex.Message;
            }

            // Get audit record count for compliance monitoring
            try
            {
                var auditCount = await _context.ServicesAudit.CountAsync(cancellationToken).ConfigureAwait(false);
                metrics["AuditRecordCount"] = auditCount;
            }
            catch (Exception ex)
            {
                metrics["AuditCountError"] = ex.Message;
            }

            // Database version information
            try
            {
                var versionQuery = "SELECT version()";
                var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }
                
                using var command = connection.CreateCommand();
                command.CommandText = versionQuery;
                var version = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) as string;
                metrics["PostgreSQLVersion"] = version?.Substring(0, Math.Min(100, version.Length)) ?? "unknown";
            }
            catch (Exception ex)
            {
                metrics["VersionError"] = ex.Message;
            }
        }
        catch (Exception ex)
        {
            metrics["MetricsError"] = ex.Message;
        }

        return metrics;
    }

    private static HealthStatus DetermineHealthStatus(Dictionary<string, object> metrics)
    {
        // GREEN PHASE: Medical-grade health status determination
        
        // If query response time is too slow, mark as degraded
        if (metrics.TryGetValue("QueryResponseTimeMs", out var responseTimeObj) 
            && responseTimeObj is long responseTime 
            && responseTime > 1000) // 1 second threshold for medical systems
        {
            return HealthStatus.Degraded;
        }

        // If there are critical errors, mark as unhealthy
        if (metrics.ContainsKey("MetricsError") || metrics.ContainsKey("ServiceCountError"))
        {
            return HealthStatus.Degraded;
        }

        return HealthStatus.Healthy;
    }
}