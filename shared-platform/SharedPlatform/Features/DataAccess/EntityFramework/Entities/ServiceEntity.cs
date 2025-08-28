using SharedPlatform.Features.DataAccess.Abstractions;

namespace SharedPlatform.Features.DataAccess.EntityFramework.Entities;

/// <summary>
/// EF Core entity implementation of IService for database persistence
/// GREEN PHASE: Concrete entity class for EF Core mapping
/// Maps to services table with medical-grade audit support
/// </summary>
public sealed class ServiceEntity : IService
{
    public Guid ServiceId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string DeliveryMode { get; set; } = string.Empty;
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // EF Core navigation and audit properties
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Medical audit record entity for compliance tracking
/// GREEN PHASE: Automatic audit trail for all service changes
/// </summary>
public sealed class ServiceAuditEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }
    public string OperationType { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public string? CorrelationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? Changes { get; set; } // JSON diff
}