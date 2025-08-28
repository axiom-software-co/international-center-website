using System.Linq.Expressions;
using SharedPlatform.Features.DomainPrimitives.Specifications;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Specifications;

public sealed class ServiceSearchSpecification : BaseSpecification<Service>
{
    private readonly string? _searchTerm;
    private readonly ServiceCategoryId? _categoryId;
    private readonly bool _includeInactive;

    public ServiceSearchSpecification(string? searchTerm = null, ServiceCategoryId? categoryId = null, bool includeInactive = false)
    {
        _searchTerm = searchTerm;
        _categoryId = categoryId;
        _includeInactive = includeInactive;
    }

    public override Expression<Func<Service, bool>> ToExpression()
    {
        return BuildCriteria(_searchTerm, _categoryId, _includeInactive);
    }

    private static Expression<Func<Service, bool>> BuildCriteria(
        string? searchTerm, 
        ServiceCategoryId? categoryId, 
        bool includeInactive)
    {
        return service =>
            (!service.IsDeleted) &&
            (includeInactive || service.PublishingStatus.Value == "published") &&
            (categoryId == null || service.CategoryId == categoryId) &&
            (string.IsNullOrWhiteSpace(searchTerm) ||
             service.Title.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
             service.Description.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public static ServiceSearchSpecification Create(string? searchTerm = null, ServiceCategoryId? categoryId = null, bool includeInactive = false)
        => new(searchTerm, categoryId, includeInactive);
}