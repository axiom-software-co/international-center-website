namespace ServicesDomain.Shared.Configuration;

/// <summary>
/// Configuration options for caching services
/// Supports Redis distributed cache with fallback configurations
/// </summary>
public sealed class CacheOptions
{
    public const string SectionName = "Cache";
    
    /// <summary>
    /// Redis connection string for distributed caching
    /// </summary>
    public string? RedisConnectionString { get; set; }
    
    /// <summary>
    /// Default cache expiration in minutes (medical-grade default: 5 minutes)
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 5;
    
    /// <summary>
    /// Sliding expiration in minutes for frequently accessed items
    /// </summary>
    public int SlidingExpirationMinutes { get; set; } = 2;
    
    /// <summary>
    /// Maximum size for in-memory fallback cache
    /// </summary>
    public int MemoryCacheSizeLimit { get; set; } = 1000;
    
    /// <summary>
    /// Memory cache compaction percentage for aggressive cleanup
    /// </summary>
    public double MemoryCacheCompactionPercentage { get; set; } = 0.25;
    
    /// <summary>
    /// Whether to use Redis distributed cache (fallback to memory if false)
    /// </summary>
    public bool UseDistributedCache { get; set; } = true;
    
    /// <summary>
    /// Redis retry count for connection failures
    /// </summary>
    public int RedisRetryCount { get; set; } = 3;
    
    /// <summary>
    /// Redis connection timeout in seconds
    /// </summary>
    public int RedisConnectionTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Validates cache configuration for medical-grade requirements
    /// </summary>
    public void Validate()
    {
        if (UseDistributedCache && string.IsNullOrWhiteSpace(RedisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is required when distributed cache is enabled");
        }
        
        if (DefaultExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Default cache expiration must be positive");
        }
        
        if (SlidingExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Sliding cache expiration must be positive");
        }
        
        if (MemoryCacheSizeLimit <= 0)
        {
            throw new InvalidOperationException("Memory cache size limit must be positive");
        }
        
        if (MemoryCacheCompactionPercentage <= 0 || MemoryCacheCompactionPercentage >= 1)
        {
            throw new InvalidOperationException("Memory cache compaction percentage must be between 0 and 1");
        }
    }
}
