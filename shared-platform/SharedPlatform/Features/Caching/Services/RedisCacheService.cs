using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SharedPlatform.Features.Caching.Abstractions;
using System.Text.Json;
using MicrosoftDistributedCache = Microsoft.Extensions.Caching.Distributed.IDistributedCache;
using MicrosoftDistributedCacheOptions = Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions;

namespace SharedPlatform.Features.Caching.Services;

/// <summary>
/// Redis-based distributed cache service implementation
/// Provides production-ready caching with medical-grade reliability and 5-minute TTL
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly MicrosoftDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private static readonly Action<ILogger, string, Exception?> _logCacheMiss =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1001, "CacheMiss"),
            "Cache miss for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheHit =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1002, "CacheHit"),
            "Cache hit for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheGetError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1003, "CacheGetError"),
            "Error retrieving value from cache for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheSetSuccess =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1004, "CacheSetSuccess"),
            "Successfully cached value for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheSetError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1005, "CacheSetError"),
            "Error setting cache value for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheExistsResult =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1006, "CacheExistsResult"),
            "Cache exists check for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheExistsError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1007, "CacheExistsError"),
            "Error checking cache existence for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheRemoveSuccess =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1008, "CacheRemoveSuccess"),
            "Removed cached value for key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logCacheRemoveError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1009, "CacheRemoveError"),
            "Error removing cached value for key: {CacheKey}");
    
    public RedisCacheService(MicrosoftDistributedCache distributedCache, ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configure JSON serialization for medical data consistency
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false, // Minimize Redis storage
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        ValidateKey(key);
        
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logCacheMiss(_logger, key, null);
                return null;
            }
            
            var deserializedValue = JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            _logCacheHit(_logger, key, null);
            return deserializedValue;
        }
        catch (JsonException ex)
        {
            _logCacheGetError(_logger, key, ex);
            // Remove corrupted cache entry for medical data integrity
            await RemoveAsync(key, cancellationToken).ConfigureAwait(false);
            return null;
        }
        catch (OperationCanceledException)
        {
            throw; // Allow cancellation to propagate
        }
        catch (InvalidOperationException ex)
        {
            _logCacheGetError(_logger, key, ex);
            return null; // Graceful degradation - don't fail the request
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ValidateKey(key);
        
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            
            // Medical-grade cache expiration settings
            var options = new MicrosoftDistributedCacheOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                // Default 5 minutes for medical data integrity
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            }
            
            // Add sliding expiration for frequently accessed data
            options.SlidingExpiration = TimeSpan.FromMinutes(2);
            
            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken).ConfigureAwait(false);
            
            _logCacheSetSuccess(_logger, key, null);
        }
        catch (JsonException ex)
        {
            _logCacheSetError(_logger, key, ex);
            throw; // Serialization failures should not be ignored for medical data
        }
        catch (OperationCanceledException)
        {
            throw; // Allow cancellation to propagate
        }
        catch (InvalidOperationException ex)
        {
            _logCacheSetError(_logger, key, ex);
            throw; // Cache failures should be visible for medical-grade systems
        }
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
            _logCacheRemoveSuccess(_logger, key, null);
        }
        catch (OperationCanceledException)
        {
            throw; // Allow cancellation to propagate
        }
        catch (InvalidOperationException ex)
        {
            _logCacheRemoveError(_logger, key, ex);
            throw; // Cache removal failures should be visible
        }
    }
    
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            var exists = !string.IsNullOrEmpty(cachedValue);
            
            _logCacheExistsResult(_logger, key, null);
            return exists;
        }
        catch (OperationCanceledException)
        {
            throw; // Allow cancellation to propagate
        }
        catch (InvalidOperationException ex)
        {
            _logCacheExistsError(_logger, key, ex);
            return false; // Assume doesn't exist on error
        }
    }
    
    /// <summary>
    /// Validates cache key format for Redis compatibility and medical compliance
    /// </summary>
    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        
        if (key.Length > 250)
            throw new ArgumentException("Cache key cannot exceed 250 characters for Redis compatibility", nameof(key));
        
        // Ensure keys are safe for Redis and medical audit trails
        if (key.Contains(' ') || key.Contains('\n') || key.Contains('\r'))
            throw new ArgumentException("Cache key cannot contain spaces or newline characters", nameof(key));
    }
}