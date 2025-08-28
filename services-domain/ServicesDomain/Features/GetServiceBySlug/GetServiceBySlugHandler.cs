using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using ServicesDomain.Features.ServiceManagement.Domain.Repository;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.GetService;
using SharedPlatform.Features.Caching.Abstractions;

namespace ServicesDomain.Features.GetServiceBySlug;

/// <summary>
/// Handler for GetServiceBySlug query with Dapper repository optimization for public API
/// Includes medical-grade audit logging and Redis caching with 5-minute TTL
/// </summary>
public sealed class GetServiceBySlugHandler : IRequestHandler<GetServiceBySlugQuery, Result<GetServiceResponse>>
{
    private readonly IServiceRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<GetServiceBySlugHandler> _logger;
    private readonly IValidator<GetServiceBySlugQuery> _validator;

    public GetServiceBySlugHandler(
        IServiceRepository repository,
        ICacheService cache,
        ILogger<GetServiceBySlugHandler> logger,
        IValidator<GetServiceBySlugQuery> validator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    private static readonly Action<ILogger, string, string, Exception?> _logOperationStarted =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(1001, "OperationStarted"),
            "GetServiceBySlug operation started for service slug {ServiceSlug} with correlation {CorrelationId}");
            
    private static readonly Action<ILogger, string, string, Exception?> _logOperationCompleted =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(1002, "OperationCompleted"),
            "GetServiceBySlug operation completed successfully for service slug {ServiceSlug} with correlation {CorrelationId}");
            
    private static readonly Action<ILogger, string, string, Exception?> _logServiceNotFound =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(1003, "ServiceNotFound"),
            "Service not found for slug {ServiceSlug} with correlation {CorrelationId}");
            
    private static readonly Action<ILogger, string, string, Exception?> _logOperationFailed =
        LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(1004, "OperationFailed"),
            "GetServiceBySlug operation failed for service slug {ServiceSlug} with correlation {CorrelationId}");
            
    private static readonly Action<ILogger, string, string, Exception?> _logValidationFailed =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(1005, "ValidationFailed"),
            "Validation failed for GetServiceBySlug with slug {ServiceSlug} and correlation {CorrelationId}");
            
    private static readonly Action<ILogger, string, string, Exception?> _logCacheHit =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(1006, "CacheHit"),
            "Cache hit for service slug {ServiceSlug} with correlation {CorrelationId}");
            
    private static readonly Action<ILogger, string, string, Exception?> _logCacheMiss =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(1007, "CacheMiss"),
            "Cache miss for service slug {ServiceSlug} with correlation {CorrelationId}");

    public async Task<Result<GetServiceResponse>> Handle(GetServiceBySlugQuery request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var slugValue = request.ServiceSlug.Value;
        
        _logOperationStarted(_logger, slugValue, correlationId, null);
        
        try
        {
            // Validate request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                _logValidationFailed(_logger, slugValue, correlationId, null);
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Result.Failure<GetServiceResponse>($"Validation failed: {errors}");
            }
            
            // Check cache first for public API performance
            var cacheKey = GenerateCacheKey(request.ServiceSlug);
            var cachedService = await _cache.GetAsync<Service>(cacheKey, cancellationToken).ConfigureAwait(false);
            
            if (cachedService != null)
            {
                _logCacheHit(_logger, slugValue, correlationId, null);
                
                // Ensure cached service is not soft-deleted
                if (cachedService.IsDeleted)
                {
                    _logServiceNotFound(_logger, slugValue, correlationId, null);
                    return Result.Failure<GetServiceResponse>("Service not found");
                }
                
                _logOperationCompleted(_logger, slugValue, correlationId, null);
                return Result.Success(GetServiceResponse.From(cachedService));
            }
            
            _logCacheMiss(_logger, slugValue, correlationId, null);
            
            // Query repository using Dapper optimization for public API
            var service = await _repository.GetBySlugAsync(request.ServiceSlug, cancellationToken).ConfigureAwait(false);
            
            if (service == null || service.IsDeleted)
            {
                _logServiceNotFound(_logger, slugValue, correlationId, null);
                return Result.Failure<GetServiceResponse>("Service not found");
            }
            
            // Cache successful result for 5 minutes (medical-grade data freshness)
            var cacheExpiration = TimeSpan.FromMinutes(5);
            await _cache.SetAsync(cacheKey, service, cacheExpiration, cancellationToken).ConfigureAwait(false);
            
            var response = GetServiceResponse.From(service);
            _logOperationCompleted(_logger, slugValue, correlationId, null);
            
            return Result.Success(response);
        }
        catch (OperationCanceledException)
        {
            throw; // Allow cancellation to propagate
        }
        catch (InvalidOperationException ex)
        {
            _logOperationFailed(_logger, slugValue, correlationId, ex);
            return Result.Failure<GetServiceResponse>("An internal error occurred while retrieving the service");
        }
#pragma warning disable CA1031 // Medical-grade safety: catch all exceptions to prevent system crashes
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logOperationFailed(_logger, slugValue, correlationId, ex);
            return Result.Failure<GetServiceResponse>("An internal error occurred while retrieving the service");
        }
    }
    
    public string GenerateCacheKey(ServiceSlug slug)
    {
        // Generate consistent cache key for slug-based lookups
        // Use SHA256 hash for cache key consistency and collision prevention
        var inputBytes = System.Text.Encoding.UTF8.GetBytes($"service:slug:{slug.Value.ToLowerInvariant()}");
        var hashBytes = System.Security.Cryptography.SHA256.HashData(inputBytes);
        var hashString = Convert.ToHexString(hashBytes)[..16]; // First 16 characters for brevity
        
        return $"svc:slug:{hashString}";
    }
}
