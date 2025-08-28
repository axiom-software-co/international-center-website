using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.ContentManagement.Abstractions;

/// <summary>
/// Content retrieval service with caching integration
/// Handles content fetching from Azure Blob Storage with Redis caching
/// </summary>
public interface IContentRetrievalService
{
    /// <summary>
    /// Retrieve content from CDN/Blob Storage with caching
    /// Integrates with existing SharedPlatform caching infrastructure
    /// </summary>
    /// <param name="contentUrl">CDN URL for content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Content retrieval result with cache information</returns>
    Task<Result<ContentRetrievalResult>> GetContentAsync(
        string contentUrl, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieve content by service ID and content hash
    /// Alternative retrieval method for internal operations
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="contentHash">Content hash</param>
    /// <param name="extension">File extension</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Content retrieval result</returns>
    Task<Result<ContentRetrievalResult>> GetContentByHashAsync(
        Guid serviceId,
        string contentHash,
        string extension,
        CancellationToken cancellationToken);

    /// <summary>
    /// Pre-warm cache with content for performance optimization
    /// </summary>
    /// <param name="contentUrl">Content URL to cache</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cache warming result</returns>
    Task<Result<bool>> PreWarmCacheAsync(
        string contentUrl, 
        CancellationToken cancellationToken);
}

/// <summary>
/// Content retrieval operation result
/// </summary>
public sealed class ContentRetrievalResult
{
    /// <summary>
    /// Retrieved content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Content MIME type
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if content was served from cache
    /// </summary>
    public bool CacheHit { get; set; }

    /// <summary>
    /// Cache key used for storage
    /// </summary>
    public string CacheKey { get; set; } = string.Empty;

    /// <summary>
    /// Content size in bytes
    /// </summary>
    public long ContentSize { get; set; }

    /// <summary>
    /// Content hash for integrity verification
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Retrieval timestamp
    /// </summary>
    public DateTime RetrievedAt { get; set; }

    /// <summary>
    /// Cache expiration time (if cached)
    /// </summary>
    public DateTime? CacheExpiresAt { get; set; }
}