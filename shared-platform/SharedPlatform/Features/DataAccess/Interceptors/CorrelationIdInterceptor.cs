using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using SharedPlatform.Features.DataAccess.EntityFramework.Entities;

namespace SharedPlatform.Features.DataAccess.Interceptors;

/// <summary>
/// High-performance EF Core interceptor for medical-grade correlation ID propagation
/// REFACTOR PHASE: Optimized implementation with proper HTTP context integration and performance monitoring
/// Features structured logging, activity tracing, and comprehensive correlation ID management
/// Medical-grade traceability with performance optimization and proper resource management
/// </summary>
public sealed class CorrelationIdInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<CorrelationIdInterceptor> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AsyncLocal<string?> _correlationId = new();

    // High-performance LoggerMessage delegates for medical-grade logging
    private static readonly Action<ILogger, string, string, Exception?> LogOperationStarted =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(2001, nameof(LogOperationStarted)),
            "Medical audit operation started: {OperationName} [{CorrelationId}]");

    private static readonly Action<ILogger, string, Exception?> LogOperationCompleted =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(2002, nameof(LogOperationCompleted)),
            "Medical audit operation completed [{CorrelationId}]");

    private static readonly Action<ILogger, int, long, Exception?> LogCorrelationPropagated =
        LoggerMessage.Define<int, long>(LogLevel.Debug, new EventId(2003, nameof(LogCorrelationPropagated)),
            "Correlation ID propagated to {AuditEntryCount} audit entries in {Duration}ms");

    private static readonly Action<ILogger, long, Exception> LogCorrelationError =
        LoggerMessage.Define<long>(LogLevel.Warning, new EventId(2004, nameof(LogCorrelationError)),
            "Correlation ID interceptor error - Duration: {Duration}ms");

    public CorrelationIdInterceptor(
        ILogger<CorrelationIdInterceptor> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: High-performance async correlation ID propagation
        if (eventData.Context != null)
        {
            PropagateCorrelationId(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        // REFACTOR PHASE: Synchronous correlation ID propagation
        if (eventData.Context != null)
        {
            PropagateCorrelationId(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    private void PropagateCorrelationId(DbContext context)
    {
        // REFACTOR PHASE: High-performance correlation ID propagation with structured logging
        using var activity = new Activity("CorrelationId.Propagate");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var correlationId = GetCurrentCorrelationId();
            
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = GenerateCorrelationId();
                SetCorrelationId(correlationId);
            }

            // High-performance enumeration with single materialization
            var auditEntries = context.ChangeTracker.Entries<ServiceAuditEntity>()
                .Where(e => e.State == EntityState.Added)
                .ToArray();

            var serviceEntries = context.ChangeTracker.Entries<ServiceEntity>()
                .Where(e => e.State != EntityState.Unchanged)
                .ToArray();

            // Propagate to audit entities
            foreach (var auditEntry in auditEntries)
            {
                if (auditEntry.Entity.CorrelationId == null)
                {
                    auditEntry.Entity.CorrelationId = Guid.TryParse(correlationId, out var corrId) ? corrId : null;
                }
            }

            // Set activity context for medical-grade tracing
            activity.SetTag("correlation.id", correlationId);
            activity.SetTag("audit.entries", auditEntries.Length.ToString());
            activity.SetTag("service.entries", serviceEntries.Length.ToString());

            LogCorrelationPropagated(_logger, auditEntries.Length, stopwatch.ElapsedMilliseconds, null);
        }
        catch (Exception ex)
        {
            // Medical-grade error handling with structured logging
            LogCorrelationError(_logger, stopwatch.ElapsedMilliseconds, ex);
            activity?.SetTag("correlation.failed", "true");
            activity?.SetTag("correlation.error", ex.Message);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private string GetCurrentCorrelationId()
    {
        // REFACTOR PHASE: High-performance correlation ID retrieval with proper HTTP context integration
        
        // 1. Check AsyncLocal first (highest priority - set by SetCorrelationId)
        if (!string.IsNullOrEmpty(_correlationId.Value))
        {
            return _correlationId.Value;
        }

        // 2. Try to get from HTTP context (for web requests)
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Check for X-Correlation-ID header
                if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
                {
                    var firstValue = headerValue.FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstValue))
                    {
                        return firstValue;
                    }
                }

                // Check for correlation ID in items
                if (httpContext.Items.TryGetValue("CorrelationId", out var itemValue) 
                    && itemValue is string correlationFromItems 
                    && !string.IsNullOrEmpty(correlationFromItems))
                {
                    return correlationFromItems;
                }

                // Check TraceIdentifier as fallback for web requests
                if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
                {
                    return httpContext.TraceIdentifier;
                }
            }
        }
        catch
        {
            // Silently handle HTTP context access errors in medical-grade systems
        }

        // 3. Check for activity/trace ID (for background operations)
        try
        {
            var activity = Activity.Current;
            if (!string.IsNullOrEmpty(activity?.Id))
            {
                return activity.Id;
            }
            
            if (!string.IsNullOrEmpty(activity?.TraceId.ToString()))
            {
                return activity.TraceId.ToString();
            }
        }
        catch
        {
            // Silently handle activity access errors
        }

        // 4. Return empty - a new correlation ID will be generated
        return string.Empty;
    }

    private static string GenerateCorrelationId()
    {
        // GREEN PHASE: Generate medical-grade correlation ID with timestamp
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters
        return $"AUDIT-{timestamp}-{guid}".ToUpperInvariant();
    }

    public void SetCorrelationId(string correlationId)
    {
        // GREEN PHASE: Set correlation ID for current operation context
        _correlationId.Value = correlationId;
    }

    public string? GetCorrelationId()
    {
        // GREEN PHASE: Public method to get current correlation ID
        return GetCurrentCorrelationId();
    }


    // Medical-grade audit compliance methods
    public void StartOperation(string operationName, string? correlationId = null)
    {
        // REFACTOR PHASE: Start a new traced operation with high-performance logging
        var finalCorrelationId = correlationId ?? GenerateCorrelationId();
        SetCorrelationId(finalCorrelationId);
        
        LogOperationStarted(_logger, operationName, finalCorrelationId, null);
    }

    public void CompleteOperation()
    {
        // REFACTOR PHASE: Complete the current operation with structured logging
        var correlationId = GetCurrentCorrelationId();
        LogOperationCompleted(_logger, correlationId, null);
        
        // Clear the correlation ID for medical-grade cleanup
        _correlationId.Value = null;
    }
}