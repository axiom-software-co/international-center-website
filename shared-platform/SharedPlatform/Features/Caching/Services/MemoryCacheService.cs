using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SharedPlatform.Features.Caching.Abstractions;

namespace SharedPlatform.Features.Caching.Services;

/// <summary>
/// In-memory cache service implementation for development/testing
/// TDD GREEN phase - minimal implementation
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    
    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
            
        var result = _cache.Get<T>(key);
        return Task.FromResult(result);
    }
    
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
            
        if (value == null)
            throw new ArgumentNullException(nameof(value));
            
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        else
        {
            // Default 5 minutes for medical data integrity
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        }
        
        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }
    
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
            
        _cache.Remove(key);
        return Task.CompletedTask;
    }
    
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
            
        var exists = _cache.TryGetValue(key, out _);
        return Task.FromResult(exists);
    }
}