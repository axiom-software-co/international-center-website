using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesDomain.Shared.Infrastructure.Data;

/// <summary>
/// Medical-grade audit entity for Service Categories table
/// Enables compliance tracking for category changes
/// </summary>
[Table("service_categories_audit")]
public sealed class ServiceCategoryAudit
{
    [Key]
    [Column("audit_id")]
    public Guid AuditId { get; set; } = Guid.NewGuid();
    
    [Column("category_id")]
    public Guid CategoryId { get; set; }
    
    [Column("operation_type")]
    [MaxLength(20)]
    public string OperationType { get; set; } = string.Empty;
    
    [Column("audit_timestamp")]
    public DateTimeOffset AuditTimestamp { get; set; }
    
    [Column("user_id")]
    [MaxLength(255)]
    public string? UserId { get; set; }
    
    [Column("correlation_id")]
    public Guid? CorrelationId { get; set; }
    
    // Snapshot of category data at time of operation
    [Column("name")]
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [Column("slug")]
    [MaxLength(255)]
    public string? Slug { get; set; }
    
    [Column("order_number")]
    public int? OrderNumber { get; set; }
    
    [Column("is_default_unassigned")]
    public bool? IsDefaultUnassigned { get; set; }
    
    [Column("created_on")]
    public DateTimeOffset? CreatedOn { get; set; }
    
    [Column("created_by")]
    [MaxLength(255)]
    public string? CreatedBy { get; set; }
    
    [Column("modified_on")]
    public DateTimeOffset? ModifiedOn { get; set; }
    
    [Column("modified_by")]
    [MaxLength(255)]
    public string? ModifiedBy { get; set; }
    
    [Column("is_deleted")]
    public bool? IsDeleted { get; set; }
    
    [Column("deleted_on")]
    public DateTimeOffset? DeletedOn { get; set; }
    
    [Column("deleted_by")]
    [MaxLength(255)]
    public string? DeletedBy { get; set; }
}