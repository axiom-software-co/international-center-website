using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.ContentManagement.Abstractions;

/// <summary>
/// Content lifecycle and cleanup management service
/// Handles orphaned content cleanup while respecting audit retention requirements
/// </summary>
public interface IContentLifecycleService
{
    /// <summary>
    /// Cleanup orphaned content past audit retention period
    /// Respects regulatory retention requirements from SERVICES-SCHEMA.md
    /// </summary>
    /// <param name="retentionPeriod">Audit retention period (e.g., 90 days)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cleanup operation result with statistics</returns>
    Task<Result<ContentCleanupResult>> CleanupOrphanedContentAsync(
        TimeSpan retentionPeriod,
        CancellationToken cancellationToken);

    /// <summary>
    /// Identify orphaned content eligible for cleanup
    /// Content not referenced by any active service records
    /// </summary>
    /// <param name="retentionPeriod">Retention period for eligibility</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orphaned content references</returns>
    Task<Result<IEnumerable<OrphanedContentReference>>> IdentifyOrphanedContentAsync(
        TimeSpan retentionPeriod,
        CancellationToken cancellationToken);

    /// <summary>
    /// Archive content before deletion for compliance
    /// Creates backup for regulatory requirements
    /// </summary>
    /// <param name="contentReferences">Content to archive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Archive operation result</returns>
    Task<Result<ContentArchiveResult>> ArchiveContentAsync(
        IEnumerable<OrphanedContentReference> contentReferences,
        CancellationToken cancellationToken);
}

/// <summary>
/// Content cleanup operation result
/// </summary>
public sealed class ContentCleanupResult
{
    /// <summary>
    /// Detailed cleanup summary with statistics
    /// </summary>
    public ContentCleanupSummary CleanupSummary { get; set; } = new();
}

/// <summary>
/// Content cleanup summary with comprehensive statistics
/// </summary>
public sealed class ContentCleanupSummary
{
    /// <summary>
    /// Total number of content items scanned
    /// </summary>
    public int ScannedCount { get; set; }

    /// <summary>
    /// Number of items eligible for cleanup based on retention
    /// </summary>
    public int EligibleForCleanupCount { get; set; }

    /// <summary>
    /// Number of items actually cleaned up
    /// </summary>
    public int CleanedCount { get; set; }

    /// <summary>
    /// Retention period used for cleanup decision
    /// </summary>
    public TimeSpan RetentionPeriodUsed { get; set; }

    /// <summary>
    /// Cleanup operation completion time
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Correlation ID for audit tracking of cleanup operation
    /// </summary>
    public Guid AuditCorrelationId { get; set; }

    /// <summary>
    /// Total storage space reclaimed in bytes
    /// </summary>
    public long StorageReclaimedBytes { get; set; }

    /// <summary>
    /// List of cleaned content references for audit
    /// </summary>
    public IEnumerable<string> CleanedContentUrls { get; set; } = [];

    /// <summary>
    /// Any errors encountered during cleanup
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = [];
}

/// <summary>
/// Reference to orphaned content eligible for cleanup
/// </summary>
public sealed class OrphanedContentReference
{
    /// <summary>
    /// Content URL
    /// </summary>
    public string ContentUrl { get; set; } = string.Empty;

    /// <summary>
    /// Blob storage path
    /// </summary>
    public string BlobPath { get; set; } = string.Empty;

    /// <summary>
    /// Content hash for verification
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Content size in bytes
    /// </summary>
    public long ContentSize { get; set; }

    /// <summary>
    /// Last referenced timestamp
    /// </summary>
    public DateTime LastReferencedAt { get; set; }

    /// <summary>
    /// Content creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Associated service ID (if available)
    /// </summary>
    public Guid? ServiceId { get; set; }
}

/// <summary>
/// Content archive operation result
/// </summary>
public sealed class ContentArchiveResult
{
    /// <summary>
    /// Number of items archived
    /// </summary>
    public int ArchivedCount { get; set; }

    /// <summary>
    /// Archive location reference
    /// </summary>
    public string ArchiveLocation { get; set; } = string.Empty;

    /// <summary>
    /// Archive completion timestamp
    /// </summary>
    public DateTime ArchivedAt { get; set; }

    /// <summary>
    /// Correlation ID for audit tracking
    /// </summary>
    public Guid AuditCorrelationId { get; set; }
}