using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using SharedPlatform.Features.DomainPrimitives.Specifications;

namespace ServicesDomain.Features.ServiceManagement.Domain.Repository;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);
    Task<Service?> GetBySlugAsync(ServiceSlug slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetBySpecificationAsync(ISpecification<Service> specification, CancellationToken cancellationToken = default);
    Task<Service> AddAsync(Service service, CancellationToken cancellationToken = default);
    Task<Service> UpdateAsync(Service service, CancellationToken cancellationToken = default);
    Task DeleteAsync(Service service, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(ServiceId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(ServiceSlug slug, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<Service>? specification = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetPagedAsync(int page, int pageSize, ISpecification<Service>? specification = null, CancellationToken cancellationToken = default);
}