using Microsoft.EntityFrameworkCore;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.ServiceManagement.Domain.Repository;
using ServicesDomain.Shared.Infrastructure.Data;
using SharedPlatform.Features.DomainPrimitives.Specifications;

namespace ServicesDomain.Shared.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of IServiceRepository
/// Production-ready implementation with PostgreSQL and medical-grade audit trails
/// </summary>
public sealed class EfServiceRepository : IServiceRepository
{
    private readonly ServicesDbContext _context;
    
    public EfServiceRepository(ServicesDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
            
        return await _context.Services
            .Where(s => !s.IsDeleted) // Respect soft delete
            .FirstOrDefaultAsync(s => s.ServiceId == id, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<Service?> GetBySlugAsync(ServiceSlug slug, CancellationToken cancellationToken = default)
    {
        if (slug == null)
            throw new ArgumentNullException(nameof(slug));
            
        return await _context.Services
            .Where(s => !s.IsDeleted) // Respect soft delete
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Where(s => !s.IsDeleted) // Respect soft delete
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<IEnumerable<Service>> GetBySpecificationAsync(ISpecification<Service> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null)
            throw new ArgumentNullException(nameof(specification));
            
        var query = _context.Services
            .Where(s => !s.IsDeleted) // Respect soft delete
            .Where(specification.ToExpression());
            
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<Service> AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
            
        await _context.Services.AddAsync(service, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false); // Triggers audit trail generation
        return service;
    }
    
    public async Task<Service> UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
            
        _context.Services.Update(service);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false); // Triggers audit trail generation
        return service;
    }
    
    public async Task DeleteAsync(Service service, CancellationToken cancellationToken = default)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
            
        // Soft delete - mark as deleted instead of removing
        service.Delete(); // Calls the entity's Delete method
        _context.Services.Update(service);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false); // Triggers audit trail generation
    }
    
    public async Task<bool> ExistsAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
            
        return await _context.Services
            .Where(s => !s.IsDeleted) // Respect soft delete
            .AnyAsync(s => s.ServiceId == id, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<bool> ExistsBySlugAsync(ServiceSlug slug, CancellationToken cancellationToken = default)
    {
        if (slug == null)
            throw new ArgumentNullException(nameof(slug));
            
        return await _context.Services
            .Where(s => !s.IsDeleted) // Respect soft delete
            .AnyAsync(s => s.Slug == slug, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<int> CountAsync(ISpecification<Service>? specification = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Services.Where(s => !s.IsDeleted); // Respect soft delete
        
        if (specification != null)
        {
            query = query.Where(specification.ToExpression());
        }
        
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<IEnumerable<Service>> GetPagedAsync(int page, int pageSize, ISpecification<Service>? specification = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Services.Where(s => !s.IsDeleted); // Respect soft delete
        
        if (specification != null)
        {
            query = query.Where(specification.ToExpression());
        }
        
        return await query
            .OrderBy(s => s.CreatedOn) // Consistent ordering for pagination
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
