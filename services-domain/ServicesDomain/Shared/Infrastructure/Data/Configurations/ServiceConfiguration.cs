using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Shared.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Service entity
/// Matches services table from SERVICES-SCHEMA.md exactly
/// </summary>
public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services", t => 
        {
            t.HasCheckConstraint("CK_services_delivery_mode", 
                "delivery_mode IN ('mobile_service', 'outpatient_service', 'inpatient_service')");
            t.HasCheckConstraint("CK_services_publishing_status",
                "publishing_status IN ('draft', 'published', 'archived')");
        });
        
        // Primary key
        builder.HasKey(s => s.ServiceId);
        builder.Property(s => s.ServiceId)
            .HasColumnName("service_id")
            .HasConversion(
                v => v.Value,
                v => ServiceId.From(v))
            .HasDefaultValueSql("gen_random_uuid()");
        
        // Value object configurations
        builder.Property(s => s.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => ServiceTitle.From(v));
                
        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => Description.From(v));
                
        builder.Property(s => s.Slug)
            .HasColumnName("slug")
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => ServiceSlug.From(v));
        builder.HasIndex(s => s.Slug).IsUnique();
        
        builder.Property(s => s.LongDescriptionUrl)
            .HasColumnName("long_description_url")
            .HasMaxLength(500)
            .HasConversion(
                v => v != null ? v.Value : null,
                v => v != null ? LongDescriptionUrl.From(v) : null);
                
        builder.Property(s => s.CategoryId)
            .HasColumnName("category_id")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => ServiceCategoryId.From(v));
                
        builder.Property(s => s.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);
            
        builder.Property(s => s.OrderNumber)
            .HasColumnName("order_number")
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(s => s.DeliveryMode)
            .HasColumnName("delivery_mode")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => DeliveryMode.From(v));
                
        builder.Property(s => s.PublishingStatus)
            .HasColumnName("publishing_status")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("draft")
            .HasConversion(
                v => v.Value,
                v => PublishingStatus.From(v));
        
        // Audit fields
        builder.Property(s => s.CreatedOn)
            .HasColumnName("created_on")
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);
            
        builder.Property(s => s.ModifiedOn)
            .HasColumnName("modified_on")
            .HasColumnType("timestamptz");
            
        builder.Property(s => s.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(255);
        
        // Soft delete fields
        builder.Property(s => s.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(s => s.DeletedOn)
            .HasColumnName("deleted_on")
            .HasColumnType("timestamptz");
            
        builder.Property(s => s.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(255);
        
        // Check constraints moved to ToTable configuration above
        
        // Foreign key to service_categories (configured when ServiceCategory is implemented)
        // This will be added once ServiceCategory entity configuration is complete
        
        // Ignore navigation properties for now (will be added in later phases)
        builder.Ignore(s => s.DomainEvents);
    }
}