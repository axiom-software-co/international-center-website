using SharedPlatform.Features.DomainPrimitives.Specifications;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Specifications;

public static class ServiceSpecifications
{
    public static BaseSpecification<Service> ByStatus(ServiceStatusType status)
        => new ExpressionSpecification<Service>(s => s.Status == status);

    public static BaseSpecification<Service> ByCategory(ServiceCategoryId categoryId)
        => new ExpressionSpecification<Service>(s => s.CategoryId == categoryId);

    public static BaseSpecification<Service> BySlug(ServiceSlug slug)
        => new ExpressionSpecification<Service>(s => s.Slug == slug);

    public static BaseSpecification<Service> ById(ServiceId serviceId)
        => new ExpressionSpecification<Service>(s => s.Id.Equals(serviceId));

    public static BaseSpecification<Service> Featured()
        => new ExpressionSpecification<Service>(s => s.IsFeatured);

    public static BaseSpecification<Service> NotDeleted()
        => new ExpressionSpecification<Service>(s => !s.Status.IsDeleted);

    public static BaseSpecification<Service> PubliclyVisible()
        => new ExpressionSpecification<Service>(s => 
            s.Status.IsActive || s.Status.IsPublished);

    public static BaseSpecification<Service> CreatedBetween(DateTime from, DateTime to)
        => new ExpressionSpecification<Service>(s => 
            s.CreatedAt >= from && s.CreatedAt <= to);

    public static BaseSpecification<Service> UpdatedSince(DateTime since)
        => new ExpressionSpecification<Service>(s => 
            s.UpdatedAt != null && s.UpdatedAt >= since);

    public static BaseSpecification<Service> WithExternalUrl()
        => new ExpressionSpecification<Service>(s => 
            !string.IsNullOrEmpty(s.ExternalUrl));

    public static BaseSpecification<Service> WithImage()
        => new ExpressionSpecification<Service>(s => 
            !string.IsNullOrEmpty(s.ImageUrl));
}