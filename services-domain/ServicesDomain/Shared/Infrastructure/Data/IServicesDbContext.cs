using Microsoft.EntityFrameworkCore;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.CategoryManagement.Domain.Entities;

namespace ServicesDomain.Shared.Infrastructure.Data;

/// <summary>
/// Interface for Services Domain DbContext - enables testing and abstraction
/// </summary>
public interface IServicesDbContext
{
    // Core domain entities
    DbSet<Service> Services { get; }
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<FeaturedCategory> FeaturedCategories { get; }
    
    // Medical-grade audit tables
    DbSet<ServiceAudit> ServicesAudit { get; }
    DbSet<ServiceCategoryAudit> ServiceCategoriesAudit { get; }
    
    // EF Core operations
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
