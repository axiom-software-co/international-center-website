using SharedPlatform.Features.ContentManagement.Abstractions;
using SharedPlatform.Features.ContentManagement.Configuration;
using SharedPlatform.Features.Caching.Abstractions;
using SharedPlatform.Features.ResultHandling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SharedPlatform.Features.ContentManagement.Services;

public sealed class ContentRetrievalService : IContentRetrievalService
{
    private readonly ICacheService _cacheService;
    private readonly IContentAuditService _auditService;
    private readonly ContentStorageOptions _options;
    private readonly ILogger<ContentRetrievalService> _logger;

    // High-performance LoggerMessage delegates
    private static readonly Action<ILogger, string, Exception?> LogCacheHit =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(4001, nameof(LogCacheHit)),
            "Content retrieved from cache for URL {ContentUrl}");

    private static readonly Action<ILogger, string, Exception?> LogCacheMiss =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(4002, nameof(LogCacheMiss)),
            "Content not in cache, simulating blob storage retrieval for URL {ContentUrl}");

    private static readonly Action<ILogger, string, Exception?> LogContentRetrieved =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(4003, nameof(LogContentRetrieved)),
            "Content retrieved from blob storage for URL {ContentUrl}");

    private static readonly Action<ILogger, string, Exception?> LogRetrievalFailed =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(4004, nameof(LogRetrievalFailed)),
            "Failed to retrieve content for URL {ContentUrl}");

    private static readonly Action<ILogger, string, Exception?> LogCacheHitByHash =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(4005, nameof(LogCacheHitByHash)),
            "Content retrieved from cache for hash {ContentHash}");

    private static readonly Action<ILogger, string, Guid, Exception?> LogRetrievedByHash =
        LoggerMessage.Define<string, Guid>(LogLevel.Information, new EventId(4006, nameof(LogRetrievedByHash)),
            "Content retrieved by hash {ContentHash} for service {ServiceId}");

    private static readonly Action<ILogger, string, Guid, Exception?> LogRetrievalByHashFailed =
        LoggerMessage.Define<string, Guid>(LogLevel.Error, new EventId(4007, nameof(LogRetrievalByHashFailed)),
            "Failed to retrieve content by hash {ContentHash} for service {ServiceId}");

    private static readonly Action<ILogger, string, Exception?> LogPreWarmSuccess =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(4008, nameof(LogPreWarmSuccess)),
            "Cache pre-warmed successfully for content URL {ContentUrl}");

    private static readonly Action<ILogger, string, string, Exception?> LogPreWarmFailed =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(4009, nameof(LogPreWarmFailed)),
            "Failed to pre-warm cache for content URL {ContentUrl}: {Error}");

    private static readonly Action<ILogger, string, Exception?> LogPreWarmException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(4010, nameof(LogPreWarmException)),
            "Failed to pre-warm cache for content URL {ContentUrl}");

    public ContentRetrievalService(
        ICacheService cacheService,
        IContentAuditService auditService,
        IOptions<ContentStorageOptions> options,
        ILogger<ContentRetrievalService> logger)
    {
        _cacheService = cacheService;
        _auditService = auditService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<ContentRetrievalResult>> GetContentAsync(string contentUrl, CancellationToken cancellationToken)
    {
        try
        {
            // Extract content hash from URL for better cache key
            var contentHash = ExtractContentHashFromUrl(contentUrl);
            var cacheKey = string.IsNullOrEmpty(contentHash) 
                ? $"content:url:{contentUrl.GetHashCode()}"
                : $"content:hash:{contentHash}";
            
            // Try cache first
            var cachedContent = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            if (cachedContent != null)
            {
                LogCacheHit(_logger, contentUrl, null);
                
                return Result<ContentRetrievalResult>.Success(new ContentRetrievalResult
                {
                    Content = cachedContent,
                    ContentType = "text/html",
                    RetrievedAt = DateTime.UtcNow,
                    CacheHit = true,
                    CacheKey = cacheKey,
                    ContentSize = cachedContent.Length,
                    ContentHash = "cached-hash"
                });
            }

            // Simulate blob storage retrieval
            LogCacheMiss(_logger, contentUrl, null);

            // Simulate retrieval delay
            await Task.Delay(10, cancellationToken);

            // Placeholder content (in production, retrieve from blob using URL)
            var content = $"<h1>Content from {contentUrl}</h1><p>Retrieved from blob storage.</p>";
            
            // Cache the retrieved content
            await _cacheService.SetAsync(cacheKey, content, TimeSpan.FromHours(1), cancellationToken);
            
            // Log retrieval for audit
            await _auditService.LogContentOperationAsync(
                Guid.Empty,
                contentUrl,
                "ContentRetrieval",
                "system",
                Guid.NewGuid(),
                cancellationToken);

            var result = new ContentRetrievalResult
            {
                Content = content,
                ContentType = "text/html",
                RetrievedAt = DateTime.UtcNow,
                CacheHit = false,
                CacheKey = cacheKey,
                ContentSize = content.Length,
                ContentHash = "retrieved-hash"
            };

            LogContentRetrieved(_logger, contentUrl, null);
            return Result<ContentRetrievalResult>.Success(result);
        }
        catch (Exception ex)
        {
            LogRetrievalFailed(_logger, contentUrl, ex);
            return Result<ContentRetrievalResult>.Failure(
                Error.Failure("ContentRetrieval.Get.Failed", 
                    $"Failed to retrieve content: {ex.Message}"));
        }
    }

    public async Task<Result<ContentRetrievalResult>> GetContentByHashAsync(Guid serviceId, string contentHash, string extension, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"content:hash:{contentHash}";
            
            // Try cache first
            var cachedContent = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            if (cachedContent != null)
            {
                LogCacheHitByHash(_logger, contentHash, null);
                
                return Result<ContentRetrievalResult>.Success(new ContentRetrievalResult
                {
                    Content = cachedContent,
                    ContentType = GetContentTypeFromExtension(extension),
                    RetrievedAt = DateTime.UtcNow,
                    CacheHit = true,
                    CacheKey = cacheKey,
                    ContentSize = cachedContent.Length,
                    ContentHash = contentHash
                });
            }

            // Simulate blob storage retrieval by hash
            await Task.Delay(10, cancellationToken);
            
            // In production, this would search for blobs with matching hash in metadata
            var content = $"<h1>Content for hash {contentHash}</h1><p>Retrieved from blob storage by hash.</p>";
            
            // Cache the retrieved content
            await _cacheService.SetAsync(cacheKey, content, TimeSpan.FromHours(1), cancellationToken);
            
            var result = new ContentRetrievalResult
            {
                Content = content,
                ContentType = GetContentTypeFromExtension(extension),
                RetrievedAt = DateTime.UtcNow,
                CacheHit = false,
                CacheKey = cacheKey,
                ContentSize = content.Length,
                ContentHash = contentHash
            };

            LogRetrievedByHash(_logger, contentHash, serviceId, null);
            return Result<ContentRetrievalResult>.Success(result);
        }
        catch (Exception ex)
        {
            LogRetrievalByHashFailed(_logger, contentHash, serviceId, ex);
            return Result<ContentRetrievalResult>.Failure(
                Error.Failure("ContentRetrieval.GetByHash.Failed", 
                    $"Failed to retrieve content by hash: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> PreWarmCacheAsync(string contentUrl, CancellationToken cancellationToken)
    {
        try
        {
            var result = await GetContentAsync(contentUrl, cancellationToken);
            
            if (result.IsSuccess)
            {
                LogPreWarmSuccess(_logger, contentUrl, null);
                return Result<bool>.Success(true);
            }
            else
            {
                LogPreWarmFailed(_logger, contentUrl, result.Error?.Message ?? "Unknown error", null);
                return Result<bool>.Success(false);
            }
        }
        catch (Exception ex)
        {
            LogPreWarmException(_logger, contentUrl, ex);
            return Result<bool>.Failure(
                Error.Failure("ContentRetrieval.PreWarm.Failed", 
                    $"Failed to pre-warm cache: {ex.Message}"));
        }
    }

    private static string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            "html" => "text/html",
            "json" => "application/json",
            "txt" => "text/plain",
            "css" => "text/css",
            "js" => "application/javascript",
            _ => "text/html"
        };
    }

    /// <summary>
    /// Extract content hash from CDN URL following SERVICES-SCHEMA.md pattern
    /// Expected pattern: https://cdn.internationalcenter.com/services/content/{service-id}/{content-hash}.{extension}
    /// </summary>
    private static string ExtractContentHashFromUrl(string contentUrl)
    {
        try
        {
            if (!Uri.TryCreate(contentUrl, UriKind.Absolute, out var uri))
                return string.Empty;

            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Expected: ["services", "content", "{service-id}", "{content-hash}.{extension}"]
            if (pathSegments.Length >= 4)
            {
                var lastSegment = pathSegments[^1]; // Get the last segment: "{content-hash}.{extension}"
                var dotIndex = lastSegment.LastIndexOf('.');
                
                if (dotIndex > 0)
                {
                    var contentHash = lastSegment[..dotIndex];
                    // Validate it looks like a hash (at least 32 characters, all alphanumeric)
                    if (contentHash.Length >= 32 && contentHash.All(c => char.IsLetterOrDigit(c)))
                    {
                        return contentHash;
                    }
                }
            }
        }
        catch
        {
            // If anything goes wrong, return empty string to fall back to URL hash
        }

        return string.Empty;
    }
}