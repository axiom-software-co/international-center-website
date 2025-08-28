using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServicesDomain.Shared.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ServiceCategoryAudit entity
/// Provides medical-grade audit trail for service category changes
/// </summary>
public sealed class ServiceCategoryAuditConfiguration : IEntityTypeConfiguration<ServiceCategoryAudit>
{
    public void Configure(EntityTypeBuilder<ServiceCategoryAudit> builder)
    {
        builder.ToTable("service_categories_audit");
        
        // Primary key
        builder.HasKey(sca => sca.AuditId);
        builder.Property(sca => sca.AuditId)
            .HasColumnName("audit_id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        // Required audit fields
        builder.Property(sca => sca.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();
            
        builder.Property(sca => sca.OperationType)
            .HasColumnName("operation_type")
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(sca => sca.AuditTimestamp)
            .HasColumnName("audit_timestamp")
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        builder.Property(sca => sca.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(255);
            
        builder.Property(sca => sca.CorrelationId)
            .HasColumnName("correlation_id");
        
        // Data snapshot fields - all nullable since audit captures point-in-time data
        builder.Property(sca => sca.Name)
            .HasColumnName("name")
            .HasMaxLength(255);
            
        builder.Property(sca => sca.Slug)
            .HasColumnName("slug")
            .HasMaxLength(255);
            
        builder.Property(sca => sca.OrderNumber)
            .HasColumnName("order_number");
            
        builder.Property(sca => sca.IsDefaultUnassigned)
            .HasColumnName("is_default_unassigned");
            
        builder.Property(sca => sca.CreatedOn)
            .HasColumnName("created_on")
            .HasColumnType("timestamptz");
            
        builder.Property(sca => sca.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);
            
        builder.Property(sca => sca.ModifiedOn)
            .HasColumnName("modified_on")
            .HasColumnType("timestamptz");
            
        builder.Property(sca => sca.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(255);
            
        builder.Property(sca => sca.IsDeleted)
            .HasColumnName("is_deleted");
            
        builder.Property(sca => sca.DeletedOn)
            .HasColumnName("deleted_on")
            .HasColumnType("timestamptz");
            
        builder.Property(sca => sca.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(255);
        
        // Check constraint for operation type
        // Note: Check constraints are not supported in EF Core 9 - handled at database level
        
        // Indexes for audit queries
        builder.HasIndex(sca => sca.CategoryId);
        builder.HasIndex(sca => sca.AuditTimestamp);
        builder.HasIndex(sca => sca.CorrelationId);
    }
}