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
    public string? LongDescriptionUrl { get; set; }
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public string? ImageUrl { get; set; }
    public int OrderNumber { get; set; } = 0;
    public string DeliveryMode { get; set; } = string.Empty;
    public string PublishingStatus { get; set; } = "draft";
    
    // Audit fields
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Soft delete fields
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IService interface compatibility
    string IService.CreatedBy => CreatedBy ?? string.Empty;
    DateTime IService.CreatedAt => CreatedOn;
    DateTime IService.UpdatedAt => ModifiedOn ?? CreatedOn;
}

/// <summary>
/// Audit record entity matching SERVICES-SCHEMA.md specification
/// Stores complete snapshots of service data for audit compliance
/// </summary>
public sealed class ServiceAuditEntity
{
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }
    public string OperationType { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public DateTime AuditTimestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public Guid? CorrelationId { get; set; }
    
    // Snapshot of data at time of operation
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? LongDescriptionUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int? OrderNumber { get; set; }
    public string? DeliveryMode { get; set; }
    public string? PublishingStatus { get; set; }
    
    // Audit fields snapshot
    public DateTime? CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Soft delete fields snapshot
    public bool? IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Service category entity matching SERVICES-SCHEMA.md specification
/// </summary>
public sealed class ServiceCategoryEntity
{
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int OrderNumber { get; set; } = 0;
    public bool IsDefaultUnassigned { get; set; } = false;
    
    // Audit fields
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Soft delete fields
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Featured categories entity matching SERVICES-SCHEMA.md specification  
/// </summary>
public sealed class FeaturedCategoryEntity
{
    public Guid FeaturedCategoryId { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public int FeaturePosition { get; set; }
    
    // Audit fields
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
}