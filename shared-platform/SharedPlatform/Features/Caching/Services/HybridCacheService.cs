using Microsoft.Extensions.Logging;
using SharedPlatform.Features.Caching.Abstractions;

namespace SharedPlatform.Features.Caching.Services;

/// <summary>
/// Hybrid cache service that uses Redis with in-memory fallback
/// Provides high availability for medical-grade systems
/// </summary>
public sealed class HybridCacheService : ICacheService
{
    private readonly ICacheService _primaryCache;
    private readonly ICacheService _fallbackCache;
    private readonly ILogger<HybridCacheService> _logger;
    
    private static readonly Action<ILogger, string, Exception?> _logPrimaryCacheFailed =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1001, "PrimaryCacheFailed"),
            "Primary cache (Redis) failed for GET operation on key: {CacheKey}, falling back to in-memory cache");
            
    private static readonly Action<ILogger, string, Exception?> _logBothCachesFailed =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1002, "BothCachesFailed"),
            "Both primary and fallback cache failed for GET operation on key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logPrimaryCacheSetFailed =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1003, "PrimaryCacheSetFailed"),
            "Primary cache (Redis) failed for SET operation on key: {CacheKey}, falling back to in-memory cache");
            
    private static readonly Action<ILogger, string, Exception?> _logBothCachesSetFailed =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1004, "BothCachesSetFailed"),
            "Both primary and fallback cache failed for SET operation on key: {CacheKey}");
            
    private static readonly Action<ILogger, string, Exception?> _logPrimaryCacheExistsFailed =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1005, "PrimaryCacheExistsFailed"),
            "Primary cache (Redis) failed for EXISTS operation on key: {CacheKey}, falling back to in-memory cache");
            
    private static readonly Action<ILogger, string, Exception?> _logBothCachesExistsFailed =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1006, "BothCachesExistsFailed"),
            "Both primary and fallback cache failed for EXISTS operation on key: {CacheKey}");
            
    private static readonly Action<ILogger, string> _logFallbackCacheUsed =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1007, "FallbackCacheUsed"),
            "Using fallback cache for key: {CacheKey} due to primary cache failure");

    public HybridCacheService(
        RedisCacheService primaryCache,
        MemoryCacheService fallbackCache,
        ILogger<HybridCacheService> logger)
    {
        _primaryCache = primaryCache ?? throw new ArgumentNullException(nameof(primaryCache));
        _fallbackCache = fallbackCache ?? throw new ArgumentNullException(nameof(fallbackCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            // Try Redis first for distributed cache consistency
            var result = await _primaryCache.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (result != null)
            {
                return result;
            }
        }
        catch (Exception ex)
        {
            _logPrimaryCacheFailed(_logger, key, ex);
        }

        try
        {
            // Fallback to in-memory cache
            return await _fallbackCache.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logBothCachesFailed(_logger, key, ex);
            return null; // Graceful degradation for medical systems
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var primarySuccess = false;
        var fallbackSuccess = false;

        // Try to set in Redis first
        try
        {
            await _primaryCache.SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
            primarySuccess = true;
        }
        catch (Exception ex)
        {
            _logPrimaryCacheSetFailed(_logger, key, ex);
        }

        // Always try to set in fallback cache for consistency
        try
        {
            await _fallbackCache.SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
            fallbackSuccess = true;
        }
        catch (Exception ex)
        {
            _logBothCachesSetFailed(_logger, key, ex);
        }

        // For medical-grade systems, at least one cache must succeed
        if (!primarySuccess && !fallbackSuccess)
        {
            throw new InvalidOperationException($"Both primary and fallback cache failed for SET operation on key: {key}");
        }

        if (!primarySuccess)
        {
            _logFallbackCacheUsed(_logger, key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Remove from both caches for consistency
        var tasks = new List<Task>();

        tasks.Add(TryRemoveFromCache(_primaryCache, "Redis", key, cancellationToken));
        tasks.Add(TryRemoveFromCache(_fallbackCache, "in-memory", key, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check Redis first for distributed consistency
            return await _primaryCache.ExistsAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logPrimaryCacheExistsFailed(_logger, key, ex);
            
            try
            {
                return await _fallbackCache.ExistsAsync(key, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception fallbackEx)
            {
                _logBothCachesExistsFailed(_logger, key, fallbackEx);
                return false;
            }
        }
    }

    private async Task TryRemoveFromCache(ICacheService cache, string cacheName, string key, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{CacheName} cache failed for REMOVE operation on key: {CacheKey}", cacheName, key);
        }
    }
}