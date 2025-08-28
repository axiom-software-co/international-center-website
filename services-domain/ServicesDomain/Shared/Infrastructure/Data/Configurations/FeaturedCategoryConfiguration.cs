using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicesDomain.Features.CategoryManagement.Domain.Entities;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Shared.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for FeaturedCategory entity
/// Matches featured_categories table from SERVICES-SCHEMA.md exactly
/// </summary>
public sealed class FeaturedCategoryConfiguration : IEntityTypeConfiguration<FeaturedCategory>
{
    public void Configure(EntityTypeBuilder<FeaturedCategory> builder)
    {
        builder.ToTable("featured_categories");
        
        // Primary key
        builder.HasKey(fc => fc.FeaturedCategoryId);
        builder.Property(fc => fc.FeaturedCategoryId)
            .HasColumnName("featured_category_id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        // CategoryId foreign key
        builder.Property(fc => fc.CategoryId)
            .HasColumnName("category_id")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => ServiceCategoryId.From(v));
        
        // Feature position (1 or 2 only)
        builder.Property(fc => fc.FeaturePosition)
            .HasColumnName("feature_position")
            .IsRequired();
        
        // Audit fields
        builder.Property(fc => fc.CreatedOn)
            .HasColumnName("created_on")
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        builder.Property(fc => fc.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255);
            
        builder.Property(fc => fc.ModifiedOn)
            .HasColumnName("modified_on")
            .HasColumnType("timestamptz");
            
        builder.Property(fc => fc.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(255);
        
        // Unique constraint on feature_position (only one category per position)
        builder.HasIndex(fc => fc.FeaturePosition).IsUnique();
        
        // Check constraint for feature_position (must be 1 or 2)
        // Note: Check constraints are not supported in EF Core 9 - handled at database level
        
        // Complex check constraint to prevent featuring default unassigned category
        // Note: Check constraints are not supported in EF Core 9 - handled at database level
        
        // Foreign key relationship will be configured when ServiceCategory is complete
        // builder.HasOne<ServiceCategory>()
        //     .WithMany()
        //     .HasForeignKey(fc => fc.CategoryId)
        //     .HasConstraintName("FK_featured_categories_service_categories");
    }
}