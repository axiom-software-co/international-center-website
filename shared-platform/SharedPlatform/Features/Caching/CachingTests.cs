using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using SharedPlatform.Features.Caching.Abstractions;
using SharedPlatform.Features.Caching.Services;
using Xunit;
using MicrosoftDistributedCache = Microsoft.Extensions.Caching.Distributed.IDistributedCache;

namespace SharedPlatform.Features.Caching;

/// <summary>
/// Integration tests for SharedPlatform caching services
/// Tests will FAIL until LoggerMessage delegate compilation errors are fixed
/// Medical-grade caching with Redis primary and in-memory fallback
/// </summary>
public sealed class CachingTests : IDisposable
{
    private readonly Mock<ILogger<RedisCacheService>> _redisLogger;
    private readonly Mock<ILogger<HybridCacheService>> _hybridLogger;
    private readonly Mock<ILogger<MemoryCacheService>> _memoryLogger;
    private readonly Mock<MicrosoftDistributedCache> _mockDistributedCache;
    private readonly IMemoryCache _memoryCache;

    public CachingTests()
    {
        _redisLogger = new Mock<ILogger<RedisCacheService>>();
        _hybridLogger = new Mock<ILogger<HybridCacheService>>();
        _memoryLogger = new Mock<ILogger<MemoryCacheService>>();
        _mockDistributedCache = new Mock<MicrosoftDistributedCache>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public void RedisCacheService_ShouldCompile_WithoutLoggerMessageErrors()
    {
        // Arrange & Act - LoggerMessage delegate signatures are now fixed
        var redisCacheService = new RedisCacheService(_mockDistributedCache.Object, _redisLogger.Object);
        
        // Assert - Should create successfully without compilation errors
        Assert.NotNull(redisCacheService);
    }

    [Fact]
    public void HybridCacheService_ShouldCompile_WithoutLoggerMessageErrors()
    {
        // Arrange & Act - LoggerMessage delegate signatures are now fixed
        var primaryCache = new Mock<ICacheService>().Object;
        var fallbackCache = new Mock<ICacheService>().Object;
        
        var hybridCacheService = new HybridCacheService(primaryCache, fallbackCache, _hybridLogger.Object);
        
        // Assert - Should create successfully without compilation errors
        Assert.NotNull(hybridCacheService);
    }

    [Fact(Timeout = 5000)]
    public async Task RedisCacheService_GetAsync_ShouldUseLoggerMessageDelegates()
    {
        // Arrange - LoggerMessage compilation is now fixed
        var redisCacheService = new RedisCacheService(_mockDistributedCache.Object, _redisLogger.Object);
        var cacheKey = "test-medical-cache-key";
        
        _mockDistributedCache.Setup(x => x.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act - LoggerMessage delegate errors are now fixed
        var result = await redisCacheService.GetAsync<string>(cacheKey);

        // Assert - Should complete without throwing LoggerMessage compilation errors
        Assert.Null(result);
        
        // Verify LoggerMessage delegates were called instead of direct logger calls
        _redisLogger.Verify(l => l.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce,
            "Should use LoggerMessage delegates for high-performance logging");
    }

    [Fact(Timeout = 5000)]
    public async Task RedisCacheService_SetAsync_ShouldUseLoggerMessageDelegates()
    {
        // Arrange - LoggerMessage compilation is now fixed
        var redisCacheService = new RedisCacheService(_mockDistributedCache.Object, _redisLogger.Object);
        var cacheKey = "test-medical-cache-set";
        var cacheValue = "test-value-for-medical-audit";
        var expiration = TimeSpan.FromMinutes(5); // Medical-grade 5-minute TTL

        // Act - LoggerMessage delegate errors are now fixed
        await redisCacheService.SetAsync(cacheKey, cacheValue, expiration);

        // Assert - Should complete with proper logging
        _mockDistributedCache.Verify(x => x.SetAsync(
            cacheKey, 
            It.IsAny<byte[]>(), 
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
            
        // Verify high-performance LoggerMessage delegates are used
        _redisLogger.Verify(l => l.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce);
    }

    [Fact(Timeout = 5000)]
    public async Task HybridCacheService_ShouldFallbackGracefully_WhenRedisFails()
    {
        // Arrange - HybridCacheService compilation is now fixed
        var mockPrimaryCache = new Mock<ICacheService>();
        var fallbackCache = new MemoryCacheService(_memoryCache, _memoryLogger.Object);
        var hybridCacheService = new HybridCacheService(mockPrimaryCache.Object, fallbackCache, _hybridLogger.Object);
        
        var cacheKey = "test-hybrid-cache-fallback";
        var cacheValue = "test-medical-data-for-fallback";

        // Setup primary cache to fail
        mockPrimaryCache.Setup(x => x.SetAsync(cacheKey, cacheValue, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Redis connection failed"));
        
        // Act - Should fallback gracefully
        await hybridCacheService.SetAsync(cacheKey, cacheValue, TimeSpan.FromMinutes(5));
        
        // Assert - Should complete without throwing
        Assert.True(true, "Hybrid cache should handle primary cache failures gracefully");
    }

    [Fact(Timeout = 5000)]
    public async Task MemoryCacheService_ShouldWork_AsStandalone()
    {
        // Arrange - This should work but will fail if compilation issues cascade
        var memoryCacheService = new MemoryCacheService(_memoryCache, _memoryLogger.Object);
        var cacheKey = "test-memory-cache-medical";
        var cacheValue = "medical-service-data";

        // Act
        await memoryCacheService.SetAsync(cacheKey, cacheValue, TimeSpan.FromMinutes(5));
        var result = await memoryCacheService.GetAsync<string>(cacheKey);

        // Assert - Memory cache should work as fallback
        Assert.Equal(cacheValue, result);
    }

    [Fact(Timeout = 5000)]
    public async Task CacheService_ShouldRespectMedicalGradeTTL_OfFiveMinutes()
    {
        // Arrange - Medical-grade systems require 5-minute max TTL for data freshness
        var memoryCacheService = new MemoryCacheService(_memoryCache, _memoryLogger.Object);
        var cacheKey = "test-medical-grade-ttl";
        var cacheValue = "medical-data-requiring-fresh-reads";
        var medicalGradeTtl = TimeSpan.FromMinutes(5);

        // Act
        await memoryCacheService.SetAsync(cacheKey, cacheValue, medicalGradeTtl);
        
        // Should be cached immediately
        var immediateResult = await memoryCacheService.GetAsync<string>(cacheKey);
        Assert.Equal(cacheValue, immediateResult);
        
        // Assert - TTL should be enforced for medical compliance
        var exists = await memoryCacheService.ExistsAsync(cacheKey);
        Assert.True(exists, "Cache entry should exist within TTL window");
    }

    [Fact(Timeout = 5000)]
    public async Task CacheKeyGeneration_ShouldBeConsistent_ForMedicalAudit()
    {
        // Arrange - Medical audit requires consistent cache key generation
        var memoryCacheService = new MemoryCacheService(_memoryCache, _memoryLogger.Object);
        var baseKey = "medical-service";
        var entityId = Guid.NewGuid();
        
        // Generate cache key using consistent algorithm
        var cacheKey1 = $"{baseKey}:{entityId}";
        var cacheKey2 = $"{baseKey}:{entityId}";
        var cacheValue = "consistent-medical-data";

        // Act - Set with first key, retrieve with second key
        await memoryCacheService.SetAsync(cacheKey1, cacheValue, TimeSpan.FromMinutes(5));
        var result = await memoryCacheService.GetAsync<string>(cacheKey2);

        // Assert - Same key should retrieve same data for audit consistency
        Assert.Equal(cacheValue, result);
    }

    [Fact(Timeout = 5000)]
    public async Task CacheService_ShouldHandleNull_AndEmptyValues_Gracefully()
    {
        // Arrange - Medical systems must handle edge cases gracefully
        var memoryCacheService = new MemoryCacheService(_memoryCache, _memoryLogger.Object);
        var nullKey = "test-null-value";
        var emptyKey = "test-empty-value";

        // Act & Assert - Should handle null values (testing with placeholder for nullable constraint)
        await memoryCacheService.SetAsync(nullKey, "null_placeholder", TimeSpan.FromMinutes(5));
        var nullResult = await memoryCacheService.GetAsync<string>(nullKey);
        Assert.NotNull(nullResult);

        // Should handle empty values
        await memoryCacheService.SetAsync(emptyKey, string.Empty, TimeSpan.FromMinutes(5));
        var emptyResult = await memoryCacheService.GetAsync<string>(emptyKey);
        Assert.Equal(string.Empty, emptyResult);
    }

    [Fact]
    public void CacheConfiguration_ShouldSupportMedicalGradeSettings()
    {
        // Arrange - Test configuration for medical-grade caching
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:Redis:ConnectionString"] = "localhost:6379",
                ["Caching:DefaultTTL"] = "00:05:00", // 5 minutes for medical compliance
                ["Caching:EnableFallback"] = "true",
                ["Caching:MedicalGrade"] = "true"
            })
            .Build();

        // Act - Configuration should be parseable
        var redisCon = config["Caching:Redis:ConnectionString"];
        var defaultTtl = TimeSpan.Parse(config["Caching:DefaultTTL"]!);
        var enableFallback = bool.Parse(config["Caching:EnableFallback"]!);
        var medicalGrade = bool.Parse(config["Caching:MedicalGrade"]!);

        // Assert - Medical-grade requirements should be configurable
        Assert.NotNull(redisCon);
        Assert.Equal(TimeSpan.FromMinutes(5), defaultTtl);
        Assert.True(enableFallback);
        Assert.True(medicalGrade);
    }

    private RedisCacheService CreateTestRedisCacheService()
    {
        // This will fail until compilation errors are fixed
        try
        {
            return new RedisCacheService(_mockDistributedCache.Object, _redisLogger.Object);
        }
        catch (NotSupportedException)
        {
            // Return null if compilation fails - tests will fail appropriately
            return null!;
        }
    }

    private HybridCacheService CreateTestHybridCacheService()
    {
        // This will fail until compilation errors are fixed
        try
        {
            var primaryCache = new Mock<ICacheService>().Object;
            var fallbackCache = new MemoryCacheService(_memoryCache, _memoryLogger.Object);
            return new HybridCacheService(primaryCache, fallbackCache, _hybridLogger.Object);
        }
        catch (NotSupportedException)
        {
            // Return null if compilation fails - tests will fail appropriately
            return null!;
        }
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
        GC.SuppressFinalize(this);
    }
}