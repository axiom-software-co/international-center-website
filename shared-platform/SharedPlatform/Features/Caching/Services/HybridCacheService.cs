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
            var result = await _primaryCache.GetAsync<T>(key, cancellationToken);
            if (result != null)
            {
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary cache (Redis) failed for GET operation on key: {CacheKey}, falling back to in-memory cache", key);
        }

        try
        {
            // Fallback to in-memory cache
            return await _fallbackCache.GetAsync<T>(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Both primary and fallback cache failed for GET operation on key: {CacheKey}", key);
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
            await _primaryCache.SetAsync(key, value, expiration, cancellationToken);
            primarySuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary cache (Redis) failed for SET operation on key: {CacheKey}", key);
        }

        // Always try to set in fallback cache for consistency
        try
        {
            await _fallbackCache.SetAsync(key, value, expiration, cancellationToken);
            fallbackSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback cache failed for SET operation on key: {CacheKey}", key);
        }

        // For medical-grade systems, at least one cache must succeed
        if (!primarySuccess && !fallbackSuccess)
        {
            throw new InvalidOperationException($"Both primary and fallback cache failed for SET operation on key: {key}");
        }

        if (!primarySuccess)
        {
            _logger.LogWarning("Only fallback cache succeeded for SET operation on key: {CacheKey}", key);
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
            return await _primaryCache.ExistsAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary cache (Redis) failed for EXISTS operation on key: {CacheKey}, checking fallback cache", key);
            
            try
            {
                return await _fallbackCache.ExistsAsync(key, cancellationToken);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Both caches failed for EXISTS operation on key: {CacheKey}", key);
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