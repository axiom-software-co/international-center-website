namespace SharedPlatform.Features.ContentManagement.Abstractions;

/// <summary>
/// CDN URL generation service following SERVICES-SCHEMA.md specification
/// Generates consistent URLs for content delivery with Azure CDN integration
/// </summary>
public interface IContentUrlGenerator
{
    /// <summary>
    /// Generate CDN URL following exact SERVICES-SCHEMA.md pattern
    /// Pattern: https://cdn.internationalcenter.com/services/content/{service-id}/{content-hash}.{extension}
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="contentHash">SHA256 content hash</param>
    /// <param name="extension">File extension (e.g., ".html")</param>
    /// <returns>Complete CDN URL for content access</returns>
    Task<string> GenerateCdnUrlAsync(Guid serviceId, string contentHash, string extension);

    /// <summary>
    /// Generate blob storage path for internal Azure Storage operations
    /// Pattern: services/content/{service-id}/{content-hash}.{extension}
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="contentHash">SHA256 content hash</param>
    /// <param name="extension">File extension</param>
    /// <returns>Blob storage path</returns>
    string GenerateBlobPath(Guid serviceId, string contentHash, string extension);

    /// <summary>
    /// Validate content URL format against SERVICES-SCHEMA.md specification
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>True if URL matches expected pattern</returns>
    bool IsValidContentUrl(string url);
}