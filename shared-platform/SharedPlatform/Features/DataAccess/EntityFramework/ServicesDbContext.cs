using Microsoft.EntityFrameworkCore;
using SharedPlatform.Features.DataAccess.EntityFramework.Entities;

namespace SharedPlatform.Features.DataAccess.EntityFramework;

/// <summary>
/// EF Core DbContext for medical-grade PostgreSQL persistence
/// GREEN PHASE: Complete implementation with medical audit support
/// PostgreSQL optimized with automatic audit trails and soft delete
/// </summary>
public sealed class ServicesDbContext : DbContext
{
    public ServicesDbContext(DbContextOptions<ServicesDbContext> options) : base(options)
    {
        // GREEN PHASE: Configured with interceptors via DI
    }

    public DbSet<ServiceEntity> Services { get; set; } = null!;
    public DbSet<ServiceAuditEntity> ServicesAudit { get; set; } = null!;
    public DbSet<ServiceCategoryEntity> ServiceCategories { get; set; } = null!;
    public DbSet<FeaturedCategoryEntity> FeaturedCategories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // GREEN PHASE: Complete PostgreSQL schema configuration
        base.OnModelCreating(modelBuilder);

        // Services table configuration
        modelBuilder.Entity<ServiceEntity>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(e => e.ServiceId);
            
            entity.Property(e => e.ServiceId)
                .HasColumnName("service_id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(e => e.Slug)
                .HasColumnName("slug")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("ix_services_slug");

            entity.Property(e => e.LongDescriptionUrl)
                .HasColumnName("long_description_url")
                .HasMaxLength(500);

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(e => e.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(500);

            entity.Property(e => e.OrderNumber)
                .HasColumnName("order_number")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.DeliveryMode)
                .HasColumnName("delivery_mode")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.PublishingStatus)
                .HasColumnName("publishing_status")
                .HasMaxLength(20)
                .HasDefaultValue("draft")
                .IsRequired();

            // Audit fields
            entity.Property(e => e.CreatedOn)
                .HasColumnName("created_on")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(255);

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on");

            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by")
                .HasMaxLength(255);

            // Soft delete fields
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.DeletedOn)
                .HasColumnName("deleted_on");

            entity.Property(e => e.DeletedBy)
                .HasColumnName("deleted_by")
                .HasMaxLength(255);

            // Indexes for performance (matching SERVICES-SCHEMA.md)
            entity.HasIndex(e => e.CategoryId)
                .HasDatabaseName("idx_services_category_id");

            entity.HasIndex(e => e.PublishingStatus)
                .HasDatabaseName("idx_services_publishing_status");

            entity.HasIndex(e => new { e.CategoryId, e.OrderNumber })
                .HasDatabaseName("idx_services_order_category");

            entity.HasIndex(e => e.DeliveryMode)
                .HasDatabaseName("idx_services_delivery_mode");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Services audit table configuration (matching SERVICES-SCHEMA.md)
        modelBuilder.Entity<ServiceAuditEntity>(entity =>
        {
            entity.ToTable("services_audit");
            entity.HasKey(e => e.AuditId);

            entity.Property(e => e.AuditId)
                .HasColumnName("audit_id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.ServiceId)
                .HasColumnName("service_id")
                .IsRequired();

            entity.Property(e => e.OperationType)
                .HasColumnName("operation_type")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.AuditTimestamp)
                .HasColumnName("audit_timestamp")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(255);

            entity.Property(e => e.CorrelationId)
                .HasColumnName("correlation_id");

            // Snapshot fields
            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasColumnType("TEXT");

            entity.Property(e => e.Slug)
                .HasColumnName("slug")
                .HasMaxLength(255);

            entity.Property(e => e.LongDescriptionUrl)
                .HasColumnName("long_description_url")
                .HasMaxLength(500);

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id");

            entity.Property(e => e.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(500);

            entity.Property(e => e.OrderNumber)
                .HasColumnName("order_number");

            entity.Property(e => e.DeliveryMode)
                .HasColumnName("delivery_mode")
                .HasMaxLength(50);

            entity.Property(e => e.PublishingStatus)
                .HasColumnName("publishing_status")
                .HasMaxLength(20);

            // Audit fields snapshot
            entity.Property(e => e.CreatedOn)
                .HasColumnName("created_on");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(255);

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on");

            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by")
                .HasMaxLength(255);

            // Soft delete fields snapshot
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted");

            entity.Property(e => e.DeletedOn)
                .HasColumnName("deleted_on");

            entity.Property(e => e.DeletedBy)
                .HasColumnName("deleted_by")
                .HasMaxLength(255);

            // Indexes
            entity.HasIndex(e => e.ServiceId)
                .HasDatabaseName("idx_services_audit_service_id");

            entity.HasIndex(e => e.AuditTimestamp)
                .HasDatabaseName("idx_services_audit_timestamp");

            entity.HasIndex(e => e.OperationType)
                .HasDatabaseName("idx_services_audit_operation");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_services_audit_user");

            entity.HasIndex(e => e.CorrelationId)
                .HasDatabaseName("idx_services_audit_correlation");
        });

        // Service categories table configuration (matching SERVICES-SCHEMA.md)
        modelBuilder.Entity<ServiceCategoryEntity>(entity =>
        {
            entity.ToTable("service_categories");
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Slug)
                .HasColumnName("slug")
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("idx_service_categories_slug");

            entity.Property(e => e.OrderNumber)
                .HasColumnName("order_number")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.IsDefaultUnassigned)
                .HasColumnName("is_default_unassigned")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedOn)
                .HasColumnName("created_on")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(255);

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on");

            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by")
                .HasMaxLength(255);

            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.DeletedOn)
                .HasColumnName("deleted_on");

            entity.Property(e => e.DeletedBy)
                .HasColumnName("deleted_by")
                .HasMaxLength(255);

            // Indexes
            entity.HasIndex(e => e.OrderNumber)
                .HasDatabaseName("idx_service_categories_order");

            entity.HasIndex(e => e.IsDefaultUnassigned)
                .HasDatabaseName("idx_service_categories_default");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Featured categories table configuration (matching SERVICES-SCHEMA.md)
        modelBuilder.Entity<FeaturedCategoryEntity>(entity =>
        {
            entity.ToTable("featured_categories");
            entity.HasKey(e => e.FeaturedCategoryId);

            entity.Property(e => e.FeaturedCategoryId)
                .HasColumnName("featured_category_id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(e => e.FeaturePosition)
                .HasColumnName("feature_position")
                .IsRequired();

            entity.Property(e => e.CreatedOn)
                .HasColumnName("created_on")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(255);

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on");

            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by")
                .HasMaxLength(255);

            // Indexes
            entity.HasIndex(e => e.CategoryId)
                .HasDatabaseName("idx_featured_categories_category_id");

            entity.HasIndex(e => e.FeaturePosition)
                .IsUnique()
                .HasDatabaseName("idx_featured_categories_position");

            // Foreign key relationship
            entity.HasOne<ServiceCategoryEntity>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // GREEN PHASE: Interceptors are configured via DI in DataAccessServiceExtensions
        base.OnConfiguring(optionsBuilder);
        
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback configuration for testing
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=internationalcenter-test;Username=postgres;Password=Password123!;Pooling=true");
        }
    }
}