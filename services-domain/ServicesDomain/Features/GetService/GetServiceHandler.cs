using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.ServiceManagement.Domain.Repository;
using SharedPlatform.Features.ResultHandling;
using SharedPlatform.Features.Caching.Abstractions;
using System.Diagnostics;

namespace ServicesDomain.Features.GetService;

/// <summary>
/// Handler for GetService query following CQRS pattern
/// Includes caching, validation, and medical-grade audit logging
/// TDD REFACTOR: High-performance logging with LoggerMessage delegates for Phase 3.4
/// </summary>
public sealed class GetServiceHandler
{
    private readonly IServiceRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<GetServiceHandler> _logger;
    private readonly IValidator<GetServiceQuery> _validator;
    
    // High-performance LoggerMessage delegates for medical-grade audit trails
    private static readonly Action<ILogger, Guid, string, Exception?> _logOperationStarted =
        LoggerMessage.Define<Guid, string>(LogLevel.Information, new EventId(1001, "OperationStarted"),
            "GetService operation started for service {ServiceId} with correlation {CorrelationId}");
            
    private static readonly Action<ILogger, long, Guid, Exception?> _logValidationCompleted =
        LoggerMessage.Define<long, Guid>(LogLevel.Debug, new EventId(1002, "ValidationCompleted"),
            "Validation completed in {ValidationDurationMs}ms for service {ServiceId}");
            
    private static readonly Action<ILogger, Guid, string, string, Exception?> _logValidationFailed =
        LoggerMessage.Define<Guid, string, string>(LogLevel.Warning, new EventId(1003, "ValidationFailed"),
            "GetService validation failed for service {ServiceId}: {ValidationErrors} [CorrelationId: {CorrelationId}]");
            
    private static readonly Action<ILogger, Guid, long, string, Exception?> _logCacheHit =
        LoggerMessage.Define<Guid, long, string>(LogLevel.Information, new EventId(1004, "CacheHit"),
            "Cache HIT for service {ServiceId} in {CacheDurationMs}ms [CorrelationId: {CorrelationId}]");
            
    private static readonly Action<ILogger, Guid, string, Exception?> _logCacheMiss =
        LoggerMessage.Define<Guid, string>(LogLevel.Debug, new EventId(1005, "CacheMiss"),
            "Cache MISS for service {ServiceId}, querying database [CorrelationId: {CorrelationId}]");
            
    private static readonly Action<ILogger, long, Guid, Exception?> _logDatabaseQueryCompleted =
        LoggerMessage.Define<long, Guid>(LogLevel.Debug, new EventId(1006, "DatabaseQueryCompleted"),
            "Database query completed in {DatabaseDurationMs}ms for service {ServiceId}");
            
    private static readonly Action<ILogger, Guid, string, Exception?> _logServiceNotFound =
        LoggerMessage.Define<Guid, string>(LogLevel.Warning, new EventId(1007, "ServiceNotFound"),
            "Service {ServiceId} not found or deleted in database [CorrelationId: {CorrelationId}]");
            
    private static readonly Action<ILogger, Guid, long, Exception?> _logServiceCached =
        LoggerMessage.Define<Guid, long>(LogLevel.Debug, new EventId(1008, "ServiceCached"),
            "Service {ServiceId} cached successfully in {CacheStoreDurationMs}ms with 5-minute TTL");
            
    private static readonly Action<ILogger, long, string, Exception?> _logOperationCompletedFromCache =
        LoggerMessage.Define<long, string>(LogLevel.Information, new EventId(1009, "OperationCompletedFromCache"),
            "GetService operation completed successfully from cache in {TotalDurationMs}ms [CorrelationId: {CorrelationId}]");
            
    private static readonly Action<ILogger, long, string, Exception?> _logOperationCompletedFromDatabase =
        LoggerMessage.Define<long, string>(LogLevel.Information, new EventId(1010, "OperationCompletedFromDatabase"),
            "GetService operation completed successfully from database in {TotalDurationMs}ms [CorrelationId: {CorrelationId}]");
            
    private static readonly Action<ILogger, Guid, long, string, Exception?> _logOperationFailed =
        LoggerMessage.Define<Guid, long, string>(LogLevel.Error, new EventId(1011, "OperationFailed"),
            "GetService operation failed for service {ServiceId} after {TotalDurationMs}ms [CorrelationId: {CorrelationId}]");
            
    private static readonly Action<ILogger, string, Guid?, string?, string, Exception?> _logAuditEventInfo =
        LoggerMessage.Define<string, Guid?, string?, string>(LogLevel.Information, new EventId(1012, "AuditEventInfo"),
            "Medical Audit Event: {EventType} for service {ServiceId} [CorrelationId: {CorrelationId}] {AuditData}");
            
    private static readonly Action<ILogger, string, Guid?, string?, string, Exception?> _logAuditEventWarning =
        LoggerMessage.Define<string, Guid?, string?, string>(LogLevel.Warning, new EventId(1013, "AuditEventWarning"),
            "Medical Audit Event: {EventType} for service {ServiceId} [CorrelationId: {CorrelationId}] {AuditData}");
    
    public GetServiceHandler(
        IServiceRepository repository,
        ICacheService cache,
        ILogger<GetServiceHandler> logger,
        IValidator<GetServiceQuery> validator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }
    
    /// <summary>
    /// Handles the GetService query with comprehensive medical-grade audit logging
    /// TDD REFACTOR phase - enhanced for production with correlation tracking
    /// </summary>
    public async Task<Result<GetServiceResponse>> Handle(GetServiceQuery query, CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var operationStart = Stopwatch.StartNew();
        
        // Medical-grade audit: Operation start
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Operation"] = "GetService",
            ["ServiceId"] = query.ServiceId.Value,
            ["Timestamp"] = DateTimeOffset.UtcNow,
            ["UserId"] = "system", // Will be enhanced with real user context
            ["Source"] = "ServicesApi"
        });
        
        _logOperationStarted(_logger, query.ServiceId.Value, correlationId, null);
            
        try
        {
            // Step 1: Medical-grade validation with audit
            var validationStart = Stopwatch.StartNew();
            var validationResult = await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
            validationStart.Stop();
            
            _logValidationCompleted(_logger, validationStart.ElapsedMilliseconds, query.ServiceId.Value, null);
            
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logValidationFailed(_logger, query.ServiceId.Value, errors, correlationId, null);
                    
                // Medical audit: Validation failure
                LogMedicalAuditEvent("VALIDATION_FAILED", correlationId, query.ServiceId.Value, new { ValidationErrors = errors });
                
                return Result<GetServiceResponse>.Failure(new Error("VALIDATION_ERROR", $"Validation failed: {errors}"));
            }
            
            // Step 2: Cache retrieval with audit
            var cacheKey = GenerateCacheKey(query.ServiceId);
            var cacheStart = Stopwatch.StartNew();
            var cachedService = await _cache.GetAsync<Service>(cacheKey, cancellationToken).ConfigureAwait(false);
            cacheStart.Stop();
            
            if (cachedService != null)
            {
                _logCacheHit(_logger, query.ServiceId.Value, cacheStart.ElapsedMilliseconds, correlationId, null);
                    
                // Medical audit: Cache hit success
                LogMedicalAuditEvent("CACHE_HIT", correlationId, query.ServiceId.Value, new 
                { 
                    CacheDurationMs = cacheStart.ElapsedMilliseconds,
                    ServiceTitle = cachedService.Title.Value,
                    PublishingStatus = cachedService.PublishingStatus.Value
                });
                
                operationStart.Stop();
                _logOperationCompletedFromCache(_logger, operationStart.ElapsedMilliseconds, correlationId, null);
                    
                return Result<GetServiceResponse>.Success(GetServiceResponse.From(cachedService));
            }
            
            _logCacheMiss(_logger, query.ServiceId.Value, correlationId, null);
            
            // Step 3: Repository query with audit
            var dbStart = Stopwatch.StartNew();
            var service = await _repository.GetByIdAsync(query.ServiceId, cancellationToken).ConfigureAwait(false);
            dbStart.Stop();
            
            _logDatabaseQueryCompleted(_logger, dbStart.ElapsedMilliseconds, query.ServiceId.Value, null);
            
            if (service == null || service.IsDeleted)
            {
                _logServiceNotFound(_logger, query.ServiceId.Value, correlationId, null);
                    
                // Medical audit: Service not found
                LogMedicalAuditEvent("SERVICE_NOT_FOUND", correlationId, query.ServiceId.Value, new 
                { 
                    DatabaseDurationMs = dbStart.ElapsedMilliseconds,
                    IsDeleted = service?.IsDeleted ?? false
                });
                
                return Result<GetServiceResponse>.Failure(new Error("NOT_FOUND", "Service not found"));
            }
            
            // Step 4: Cache storage with audit
            var cacheStoreStart = Stopwatch.StartNew();
            await _cache.SetAsync(cacheKey, service, TimeSpan.FromMinutes(5), cancellationToken).ConfigureAwait(false);
            cacheStoreStart.Stop();
            
            _logServiceCached(_logger, query.ServiceId.Value, cacheStoreStart.ElapsedMilliseconds, null);
            
            // Medical audit: Successful retrieval
            LogMedicalAuditEvent("SERVICE_RETRIEVED", correlationId, query.ServiceId.Value, new 
            { 
                ServiceTitle = service.Title.Value,
                PublishingStatus = service.PublishingStatus.Value,
                DatabaseDurationMs = dbStart.ElapsedMilliseconds,
                CacheStoreDurationMs = cacheStoreStart.ElapsedMilliseconds,
                Source = "Database"
            });
            
            operationStart.Stop();
            _logOperationCompletedFromDatabase(_logger, operationStart.ElapsedMilliseconds, correlationId, null);
            
            return Result<GetServiceResponse>.Success(GetServiceResponse.From(service));
        }
        catch (InvalidOperationException ex)
        {
            operationStart.Stop();
            _logOperationFailed(_logger, query.ServiceId.Value, operationStart.ElapsedMilliseconds, correlationId, ex);
                
            // Medical audit: Operation failure
            LogMedicalAuditEvent("OPERATION_FAILED", correlationId, query.ServiceId.Value, new 
            { 
                ExceptionType = ex.GetType().Name,
                ExceptionMessage = ex.Message,
                TotalDurationMs = operationStart.ElapsedMilliseconds
            });
            
            return Result<GetServiceResponse>.Failure(new Error("INTERNAL_ERROR", "An internal error occurred while retrieving the service"));
        }
        catch (TimeoutException ex)
        {
            operationStart.Stop();
            _logOperationFailed(_logger, query.ServiceId.Value, operationStart.ElapsedMilliseconds, correlationId, ex);
                
            // Medical audit: Operation failure
            LogMedicalAuditEvent("OPERATION_TIMEOUT", correlationId, query.ServiceId.Value, new 
            { 
                ExceptionType = ex.GetType().Name,
                ExceptionMessage = ex.Message,
                TotalDurationMs = operationStart.ElapsedMilliseconds
            });
            
            return Result<GetServiceResponse>.Failure(new Error("TIMEOUT_ERROR", "Operation timed out while retrieving the service"));
        }
        catch (TaskCanceledException ex)
        {
            operationStart.Stop();
            _logOperationFailed(_logger, query.ServiceId.Value, operationStart.ElapsedMilliseconds, correlationId, ex);
                
            // Medical audit: Operation failure
            LogMedicalAuditEvent("OPERATION_FAILED", correlationId, query.ServiceId.Value, new 
            { 
                ExceptionType = ex.GetType().Name,
                ExceptionMessage = ex.Message,
                TotalDurationMs = operationStart.ElapsedMilliseconds
            });
            
            return Result<GetServiceResponse>.Failure(new Error("INTERNAL_ERROR", "An internal error occurred while retrieving the service"));
        }
    }
    
    /// <summary>
    /// Generates consistent cache key for service
    /// TDD GREEN phase - minimal implementation to make tests pass
    /// </summary>
    public string GenerateCacheKey(ServiceId serviceId)
    {
        if (serviceId == null)
            throw new ArgumentNullException(nameof(serviceId));
            
        return $"service:{serviceId.Value}";
    }
    
    /// <summary>
    /// Logs medical-grade audit events with structured data for compliance
    /// Includes correlation ID, timestamps, and operation details
    /// </summary>
    private void LogMedicalAuditEvent(string eventType, string correlationId, Guid serviceId, object additionalData)
    {
        var auditEvent = new
        {
            AuditEventType = eventType,
            CorrelationId = correlationId,
            ServiceId = serviceId,
            Timestamp = DateTimeOffset.UtcNow,
            UserId = "system", // Will be enhanced with real user context from HTTP context
            Operation = "GetService",
            Source = "ServicesApi.GetService",
            AdditionalData = additionalData
        };
        
        // Log as Information for successful events, Warning for failures
        var auditData = System.Text.Json.JsonSerializer.Serialize(auditEvent);
        var isFailureEvent = eventType.Contains("FAILED") || eventType.Contains("NOT_FOUND");
        
        if (isFailureEvent)
        {
            _logAuditEventWarning(_logger, eventType, serviceId, correlationId, auditData, null);
        }
        else
        {
            _logAuditEventInfo(_logger, eventType, serviceId, correlationId, auditData, null);
        }
    }
}
