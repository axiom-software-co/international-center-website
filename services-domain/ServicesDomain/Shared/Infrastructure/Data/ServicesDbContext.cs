using Microsoft.EntityFrameworkCore;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.CategoryManagement.Domain.Entities;
using ServicesDomain.Shared.Infrastructure.Data.Configurations;
using System.Diagnostics;

namespace ServicesDomain.Shared.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for Services Domain
/// Implements medical-grade audit trails and matches SERVICES-SCHEMA.md exactly
/// </summary>
public sealed class ServicesDbContext : DbContext, IServicesDbContext
{
    private string? _currentCorrelationId;
    
    public ServicesDbContext(DbContextOptions<ServicesDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Sets correlation ID for medical-grade audit trail tracking
    /// </summary>
    public void SetCorrelationId(string correlationId)
    {
        _currentCorrelationId = correlationId;
    }
    
    // Core domain entities
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;
    public DbSet<FeaturedCategory> FeaturedCategories { get; set; } = null!;
    
    // Medical-grade audit tables  
    public DbSet<ServiceAudit> ServicesAudit { get; set; } = null!;
    public DbSet<ServiceCategoryAudit> ServiceCategoriesAudit { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply entity configurations that match SERVICES-SCHEMA.md
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new FeaturedCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceAuditConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceCategoryAuditConfiguration());
        
        // Configure PostgreSQL-specific settings
        ConfigurePostgreSqlSettings(modelBuilder);
    }
    
    private static void ConfigurePostgreSqlSettings(ModelBuilder modelBuilder)
    {
        // Use PostgreSQL naming conventions (snake_case)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
            
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }
        }
        
        // Configure UUID generation for PostgreSQL
        modelBuilder.HasPostgresExtension("pgcrypto");
    }
    
    /// <summary>
    /// Saves changes with automatic audit trail generation for medical compliance
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await GenerateAuditEntries().ConfigureAwait(false);
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Generates audit trail entries for medical-grade compliance
    /// </summary>
    private async Task GenerateAuditEntries()
    {
        var entries = ChangeTracker.Entries<Service>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
            
        foreach (var entry in entries)
        {
            var auditEntry = new ServiceAudit
            {
                ServiceId = entry.Entity.ServiceId.Value,
                OperationType = entry.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                },
                AuditTimestamp = DateTimeOffset.UtcNow,
                CorrelationId = GetCorrelationIdForAudit(),
                
                // Snapshot current values for audit trail
                Title = entry.Entity.Title.Value,
                Description = entry.Entity.Description.Value,
                Slug = entry.Entity.Slug.Value,
                LongDescriptionUrl = entry.Entity.LongDescriptionUrl?.Value,
                CategoryId = entry.Entity.CategoryId.Value,
                ImageUrl = entry.Entity.ImageUrl,
                OrderNumber = entry.Entity.OrderNumber,
                DeliveryMode = entry.Entity.DeliveryMode.Value,
                PublishingStatus = entry.Entity.PublishingStatus.Value,
                IsDeleted = entry.Entity.IsDeleted,
                CreatedOn = entry.Entity.CreatedOn,
                CreatedBy = entry.Entity.CreatedBy,
                ModifiedOn = entry.Entity.ModifiedOn,
                ModifiedBy = entry.Entity.ModifiedBy,
                DeletedOn = entry.Entity.DeletedOn,
                DeletedBy = entry.Entity.DeletedBy
            };
            
            await ServicesAudit.AddAsync(auditEntry).ConfigureAwait(false);
        }
    }
    
    /// <summary>
    /// Gets correlation ID for medical-grade audit trail
    /// Uses Activity.Current for distributed tracing or falls back to provided correlation ID
    /// </summary>
    private Guid GetCorrelationIdForAudit()
    {
        // Priority 1: Use explicitly set correlation ID
        if (!string.IsNullOrEmpty(_currentCorrelationId) && Guid.TryParse(_currentCorrelationId, out var explicitId))
        {
            return explicitId;
        }
        
        // Priority 2: Use Activity.Current ID (distributed tracing)
        var activityId = Activity.Current?.Id;
        if (!string.IsNullOrEmpty(activityId))
        {
            // Create deterministic GUID from Activity ID for audit consistency
            return CreateGuidFromString(activityId);
        }
        
        // Fallback: Generate new GUID (should be rare in production)
        return Guid.NewGuid();
    }
    
    /// <summary>
    /// Creates deterministic GUID from string for audit trail consistency
    /// Uses SHA256 for security compliance
    /// </summary>
    private static Guid CreateGuidFromString(string input)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        // Take first 16 bytes for GUID construction
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }
}

/// <summary>
/// Extension method for converting PascalCase to snake_case for PostgreSQL
/// </summary>
internal static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
            
        return System.Text.RegularExpressions.Regex.Replace(input, 
            "([a-z0-9])([A-Z])", 
            "$1_$2")
            .ToLowerInvariant();
    }
}
