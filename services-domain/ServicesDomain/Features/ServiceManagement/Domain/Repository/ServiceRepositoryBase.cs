using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using SharedPlatform.Features.DomainPrimitives.Specifications;

namespace ServicesDomain.Features.ServiceManagement.Domain.Repository;

public abstract class ServiceRepositoryBase : IServiceRepository
{
    public abstract Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);
    public abstract Task<Service?> GetBySlugAsync(ServiceSlug slug, CancellationToken cancellationToken = default);
    public abstract Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default);
    public abstract Task<IEnumerable<Service>> GetBySpecificationAsync(ISpecification<Service> specification, CancellationToken cancellationToken = default);
    public abstract Task<Service> AddAsync(Service service, CancellationToken cancellationToken = default);
    public abstract Task<Service> UpdateAsync(Service service, CancellationToken cancellationToken = default);
    public abstract Task DeleteAsync(Service service, CancellationToken cancellationToken = default);
    public abstract Task<bool> ExistsAsync(ServiceId id, CancellationToken cancellationToken = default);
    public abstract Task<bool> ExistsBySlugAsync(ServiceSlug slug, CancellationToken cancellationToken = default);
    public abstract Task<int> CountAsync(ISpecification<Service>? specification = null, CancellationToken cancellationToken = default);
    public abstract Task<IEnumerable<Service>> GetPagedAsync(int page, int pageSize, ISpecification<Service>? specification = null, CancellationToken cancellationToken = default);

    protected virtual void ValidateService(Service service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (service.Id == null)
            throw new ArgumentException("Service ID cannot be null", nameof(service));

        if (string.IsNullOrWhiteSpace(service.Title))
            throw new ArgumentException("Service title cannot be null or empty", nameof(service));

        if (string.IsNullOrWhiteSpace(service.Description))
            throw new ArgumentException("Service description cannot be null or empty", nameof(service));

        if (string.IsNullOrWhiteSpace(service.Slug))
            throw new ArgumentException("Service slug cannot be null or empty", nameof(service));
    }

    protected virtual void ValidatePageParameters(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0", nameof(page));

        if (pageSize < 1 || pageSize > 1000)
            throw new ArgumentException("Page size must be between 1 and 1000", nameof(pageSize));
    }
}