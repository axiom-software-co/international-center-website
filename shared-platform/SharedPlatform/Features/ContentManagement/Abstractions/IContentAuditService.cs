using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.ContentManagement.Abstractions;

/// <summary>
/// Content audit logging service for compliance requirements
/// Integrates with existing audit infrastructure for content operations
/// </summary>
public interface IContentAuditService
{
    /// <summary>
    /// Log content operation for audit compliance
    /// Integrates with existing MedicalAuditInterceptor patterns
    /// </summary>
    /// <param name="serviceId">Service associated with content</param>
    /// <param name="contentUrl">Content URL for audit trail</param>
    /// <param name="operation">Operation type (UPLOAD, UPDATE, DELETE)</param>
    /// <param name="userId">User performing operation</param>
    /// <param name="correlationId">Correlation ID for operation tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit entry result with audit record details</returns>
    Task<Result<ContentAuditEntry>> LogContentOperationAsync(
        Guid serviceId,
        string contentUrl,
        string operation,
        string userId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create audit context for content operations
    /// Provides structured audit information for compliance
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="contentHash">Content hash for verification</param>
    /// <param name="operation">Operation being performed</param>
    /// <param name="userId">User context</param>
    /// <returns>Structured audit context</returns>
    Task<ContentAuditContext> CreateAuditContextAsync(
        Guid serviceId,
        string contentHash,
        string operation,
        string userId);
}

/// <summary>
/// Content audit entry for compliance tracking
/// </summary>
public sealed class ContentAuditEntry
{
    /// <summary>
    /// Unique audit record identifier
    /// </summary>
    public Guid AuditId { get; set; }

    /// <summary>
    /// Service associated with content operation
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Content URL for audit trail
    /// </summary>
    public string ContentUrl { get; set; } = string.Empty;

    /// <summary>
    /// Operation type (UPLOAD, UPDATE, DELETE)
    /// </summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// User who performed the operation
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for operation tracking
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Audit timestamp
    /// </summary>
    public DateTime AuditTimestamp { get; set; }

    /// <summary>
    /// Additional audit context information
    /// </summary>
    public object? AuditContext { get; set; }
}

/// <summary>
/// Structured audit context for content operations
/// </summary>
public sealed class ContentAuditContext
{
    /// <summary>
    /// Content hash for integrity verification
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Content size for monitoring
    /// </summary>
    public long ContentSize { get; set; }

    /// <summary>
    /// Content type for classification
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Operation metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Compliance tags for regulatory requirements
    /// </summary>
    public string[] ComplianceTags { get; set; } = [];
}