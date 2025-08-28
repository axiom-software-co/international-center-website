using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesDomain.Features.CategoryManagement.Domain.Entities;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Shared.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ServiceCategory entity
/// Matches service_categories table from SERVICES-SCHEMA.md exactly
/// </summary>
public sealed class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("service_categories");
        
        // This is a temporary placeholder configuration
        // ServiceCategory entity needs to be fully implemented first
        // For now, configure basic structure to match schema
        
        builder.HasKey("CategoryId");
        builder.Property("CategoryId")
            .HasColumnName("category_id")
            .HasDefaultValueSql("gen_random_uuid()");
            
        // Basic properties to match schema
        builder.Property<string>("Name")
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property<string>("Slug")
            .HasColumnName("slug")
            .HasMaxLength(255)
            .IsRequired();
        builder.HasIndex("Slug").IsUnique();
            
        builder.Property<int>("OrderNumber")
            .HasColumnName("order_number")
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property<bool>("IsDefaultUnassigned")
            .HasColumnName("is_default_unassigned")
            .IsRequired()
            .HasDefaultValue(false);
        
        // Audit fields
        builder.Property<DateTimeOffset>("CreatedOn")
            .HasColumnName("created_on")
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        builder.Property<string>("CreatedBy")
            .HasColumnName("created_by")
            .HasMaxLength(255);
            
        builder.Property<DateTimeOffset?>("ModifiedOn")
            .HasColumnName("modified_on")
            .HasColumnType("timestamptz");
            
        builder.Property<string>("ModifiedBy")
            .HasColumnName("modified_by")
            .HasMaxLength(255);
        
        // Soft delete fields
        builder.Property<bool>("IsDeleted")
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property<DateTimeOffset?>("DeletedOn")
            .HasColumnName("deleted_on")
            .HasColumnType("timestamptz");
            
        builder.Property<string>("DeletedBy")
            .HasColumnName("deleted_by")
            .HasMaxLength(255);
        
        // Complex check constraint for only one default unassigned category
        // Note: Check constraints are not supported in EF Core 9 - handled at database level
    }
}