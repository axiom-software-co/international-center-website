using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServicesDomain.Shared.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ServiceAudit entity
/// Matches services_audit table from SERVICES-SCHEMA.md exactly
/// </summary>
public sealed class ServiceAuditConfiguration : IEntityTypeConfiguration<ServiceAudit>
{
    public void Configure(EntityTypeBuilder<ServiceAudit> builder)
    {
        builder.ToTable("services_audit");
        
        // Primary key
        builder.HasKey(sa => sa.AuditId);
        builder.Property(sa => sa.AuditId)
            .HasColumnName("audit_id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        // Required audit fields
        builder.Property(sa => sa.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();
            
        builder.Property(sa => sa.OperationType)
            .HasColumnName("operation_type")
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(sa => sa.AuditTimestamp)
            .HasColumnName("audit_timestamp")
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        builder.Property(sa => sa.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(255);
            
        builder.Property(sa => sa.CorrelationId)
            .HasColumnName("correlation_id");
        
        // Data snapshot fields - all nullable since audit captures point-in-time data
        builder.Property(sa => sa.Title)
            .HasColumnName("title")
            .HasMaxLength(255);
            
        builder.Property(sa => sa.Description)
            .HasColumnName("description")
            .HasColumnType("text");
            
        builder.Property(sa => sa.Slug)
            .HasColumnName("slug")
            .HasMaxLength(255);
            
        builder.Property(sa => sa.LongDescriptionUrl)
            .HasColumnName("long_description_url")
            .HasMaxLength(500);
            
        builder.Property(sa => sa.CategoryId)
            .HasColumnName("category_id");
            
        builder.Property(sa => sa.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);
            
        builder.Property(sa => sa.OrderNumber)
            .HasColumnName("order_number");
            
        builder.Property(sa => sa.DeliveryMode)
            .HasColumnName("delivery_mode")
            .HasMaxLength(50);
            
        builder.Property(sa => sa.PublishingStatus)
            .HasColumnName("publishing_status")
            .HasMaxLength(20);
            
        builder.Property(sa => sa.CreatedOn)
            .HasColumnName("created_on")
            .HasColumnType("timestamptz");
            
        builder.Property(sa => sa.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);
            
        builder.Property(sa => sa.ModifiedOn)
            .HasColumnName("modified_on")
            .HasColumnType("timestamptz");
            
        builder.Property(sa => sa.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(255);
            
        builder.Property(sa => sa.IsDeleted)
            .HasColumnName("is_deleted");
            
        builder.Property(sa => sa.DeletedOn)
            .HasColumnName("deleted_on")
            .HasColumnType("timestamptz");
            
        builder.Property(sa => sa.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(255);
        
        // Check constraint for operation type
        // Note: Check constraints are not supported in EF Core 9 - handled at database level
        
        // Indexes for audit queries
        builder.HasIndex(sa => sa.ServiceId);
        builder.HasIndex(sa => sa.AuditTimestamp);
        builder.HasIndex(sa => sa.CorrelationId);
    }
}