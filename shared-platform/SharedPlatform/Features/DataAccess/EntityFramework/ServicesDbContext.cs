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

            entity.Property(e => e.DeliveryMode)
                .HasColumnName("delivery_mode")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            entity.Property(e => e.DeletedBy)
                .HasColumnName("deleted_by")
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(100);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Services audit table configuration
        modelBuilder.Entity<ServiceAuditEntity>(entity =>
        {
            entity.ToTable("services_audit");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.ServiceId)
                .HasColumnName("service_id")
                .IsRequired();

            entity.Property(e => e.OperationType)
                .HasColumnName("operation_type")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(50);

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.Property(e => e.OldValues)
                .HasColumnName("old_values")
                .HasColumnType("jsonb");

            entity.Property(e => e.NewValues)
                .HasColumnName("new_values")
                .HasColumnType("jsonb");

            entity.Property(e => e.Changes)
                .HasColumnName("changes")
                .HasColumnType("jsonb");

            entity.HasIndex(e => e.ServiceId)
                .HasDatabaseName("ix_services_audit_service_id");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("ix_services_audit_timestamp");
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