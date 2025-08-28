using SharedPlatform.Features.ResultHandling;
using SharedPlatform.Features.DataAccess.Abstractions;

namespace SharedPlatform.Features.ContentManagement.Abstractions;

/// <summary>
/// Azure Blob Storage content management service
/// Handles content upload, versioning, and URL generation following SERVICES-SCHEMA.md specification
/// </summary>
public interface IContentStorageService
{
    /// <summary>
    /// Upload content to Azure Blob Storage with hash-based URL generation
    /// </summary>
    /// <param name="serviceId">Service identifier for content organization</param>
    /// <param name="content">HTML/text content to upload</param>
    /// <param name="contentType">MIME type of content</param>
    /// <param name="userId">User performing the upload for audit</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result with CDN URL and metadata</returns>
    Task<Result<ContentUploadResult>> UploadContentAsync(
        Guid serviceId, 
        string content, 
        string contentType, 
        string userId, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Upload content and update the associated service record with content URL
    /// Integrates with existing DataAccess infrastructure for service updates
    /// </summary>
    /// <param name="serviceId">Service to update with content URL</param>
    /// <param name="content">HTML/text content to upload</param>
    /// <param name="contentType">MIME type of content</param>
    /// <param name="userId">User performing the operation for audit</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result with service update confirmation</returns>
    Task<Result<ContentUploadResult>> UploadAndUpdateServiceAsync(
        IServiceId serviceId, 
        string content, 
        string contentType, 
        string userId, 
        CancellationToken cancellationToken);
}

/// <summary>
/// Content upload operation result
/// </summary>
public sealed class ContentUploadResult
{
    /// <summary>
    /// CDN URL for content access following SERVICES-SCHEMA.md pattern
    /// Format: https://cdn.internationalcenter.com/services/content/{service-id}/{content-hash}.html
    /// </summary>
    public string ContentUrl { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 hash of content for immutability and cache-busting
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Blob storage path for internal reference
    /// Format: services/content/{service-id}/{content-hash}.html
    /// </summary>
    public string BlobPath { get; set; } = string.Empty;

    /// <summary>
    /// Upload timestamp for audit compliance
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// User who performed the upload for audit tracking
    /// </summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>
    /// Content size in bytes for monitoring and billing
    /// </summary>
    public long ContentSize { get; set; }
}