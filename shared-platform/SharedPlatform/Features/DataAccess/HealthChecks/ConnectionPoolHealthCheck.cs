using Microsoft.Extensions.Diagnostics.HealthChecks;
using SharedPlatform.Features.DataAccess.Dapper;
using System.Data;
using Dapper;

namespace SharedPlatform.Features.DataAccess.HealthChecks;

/// <summary>
/// Health check for connection pool performance monitoring
/// GREEN PHASE: Complete implementation with medical-grade monitoring
/// Comprehensive connection pool efficiency and performance monitoring for public APIs
/// </summary>
public sealed class ConnectionPoolHealthCheck : IHealthCheck
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ConnectionPoolHealthCheck(DapperConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Complete connection pool health monitoring
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test connection pool functionality
            var poolEfficient = await TestPoolEfficiency(cancellationToken).ConfigureAwait(false);
            if (!poolEfficient)
            {
                return HealthCheckResult.Unhealthy("Connection pool is not functioning efficiently");
            }

            // Get comprehensive pool metrics
            var metrics = await GetConnectionPoolMetrics(cancellationToken).ConfigureAwait(false);
            
            // Measure connection creation time
            var connectionTime = await MeasureConnectionCreationTime(cancellationToken).ConfigureAwait(false);
            metrics["ConnectionCreationTimeMs"] = connectionTime.TotalMilliseconds;
            
            stopwatch.Stop();
            metrics["HealthCheckDurationMs"] = stopwatch.ElapsedMilliseconds;

            // Determine health status based on performance metrics
            var status = DeterminePoolHealthStatus(metrics);
            var description = $"Connection pool health check completed in {stopwatch.ElapsedMilliseconds}ms";

            return new HealthCheckResult(status, description, data: metrics);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Connection pool health check failed: {ex.Message}", ex);
        }
    }

    private async Task<Dictionary<string, object>> GetConnectionPoolMetrics(CancellationToken cancellationToken)
    {
        // GREEN PHASE: Comprehensive connection pool metrics
        var metrics = new Dictionary<string, object>();

        try
        {
            // Get pool configuration metrics from factory
            var poolConfig = _connectionFactory.GetConnectionPoolMetrics();
            foreach (var kvp in poolConfig)
            {
                metrics[kvp.Key] = kvp.Value;
            }

            // Test multiple concurrent connections to validate pooling
            var concurrentConnectionTasks = Enumerable.Range(0, 5)
                .Select(async _ =>
                {
                    var connectionStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
                        connectionStopwatch.Stop();
                        return connectionStopwatch.Elapsed;
                    }
                    catch
                    {
                        return TimeSpan.MaxValue; // Indicate failure
                    }
                });

            var connectionTimes = await Task.WhenAll(concurrentConnectionTasks).ConfigureAwait(false);
            
            metrics["ConcurrentConnectionsTestedCount"] = connectionTimes.Length;
            metrics["SuccessfulConnectionsCount"] = connectionTimes.Count(t => t != TimeSpan.MaxValue);
            metrics["FailedConnectionsCount"] = connectionTimes.Count(t => t == TimeSpan.MaxValue);
            
            if (connectionTimes.Any(t => t != TimeSpan.MaxValue))
            {
                var validTimes = connectionTimes.Where(t => t != TimeSpan.MaxValue).ToList();
                metrics["AverageConnectionTimeMs"] = validTimes.Average(t => t.TotalMilliseconds);
                metrics["MaxConnectionTimeMs"] = validTimes.Max(t => t.TotalMilliseconds);
                metrics["MinConnectionTimeMs"] = validTimes.Min(t => t.TotalMilliseconds);
            }

            // Test connection functionality
            var functionalityStopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var testConn = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
                await testConn.QueryFirstAsync<int>("SELECT 1", cancellationToken).ConfigureAwait(false);
                functionalityStopwatch.Stop();
                metrics["FunctionalityTestMs"] = functionalityStopwatch.ElapsedMilliseconds;
                metrics["FunctionalityTestResult"] = "Success";
            }
            catch (Exception ex)
            {
                functionalityStopwatch.Stop();
                metrics["FunctionalityTestMs"] = functionalityStopwatch.ElapsedMilliseconds;
                metrics["FunctionalityTestResult"] = $"Failed: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            metrics["MetricsCollectionError"] = ex.Message;
        }

        return metrics;
    }

    private async Task<TimeSpan> MeasureConnectionCreationTime(CancellationToken cancellationToken)
    {
        // GREEN PHASE: Performance measurement for connection creation
        try
        {
            return await _connectionFactory.MeasureConnectionCreationTimeAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return TimeSpan.MaxValue; // Indicate measurement failure
        }
    }

    private async Task<bool> TestPoolEfficiency(CancellationToken cancellationToken)
    {
        // GREEN PHASE: Test connection pool efficiency and reuse
        try
        {
            // Create multiple connections sequentially to test pool reuse
            const int testConnections = 3;
            var connectionTimes = new List<TimeSpan>();

            for (int i = 0; i < testConnections; i++)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
                
                // Perform a simple query to ensure connection is functional
                await conn.QueryFirstAsync<int>("SELECT 1", cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                connectionTimes.Add(stopwatch.Elapsed);
            }

            // Pool is efficient if connections are created reasonably quickly
            // Medical systems require sub-100ms connection times for performance
            return connectionTimes.All(time => time.TotalMilliseconds < 100);
        }
        catch
        {
            return false;
        }
    }

    private static HealthStatus DeterminePoolHealthStatus(Dictionary<string, object> metrics)
    {
        // GREEN PHASE: Medical-grade health status determination for connection pool
        
        // Check for critical failures
        if (metrics.TryGetValue("FailedConnectionsCount", out var failedObj) 
            && failedObj is int failedCount 
            && failedCount > 0)
        {
            return HealthStatus.Unhealthy;
        }

        // Check connection creation performance (medical systems need fast responses)
        if (metrics.TryGetValue("ConnectionCreationTimeMs", out var creationTimeObj) 
            && creationTimeObj is double creationTime 
            && creationTime > 500) // 500ms threshold
        {
            return HealthStatus.Degraded;
        }

        // Check average connection time performance
        if (metrics.TryGetValue("AverageConnectionTimeMs", out var avgTimeObj) 
            && avgTimeObj is double avgTime 
            && avgTime > 100) // 100ms threshold for medical systems
        {
            return HealthStatus.Degraded;
        }

        // Check for functionality test failures
        if (metrics.TryGetValue("FunctionalityTestResult", out var funcResultObj) 
            && funcResultObj is string funcResult 
            && !funcResult.Equals("Success", StringComparison.OrdinalIgnoreCase))
        {
            return HealthStatus.Unhealthy;
        }

        return HealthStatus.Healthy;
    }
}