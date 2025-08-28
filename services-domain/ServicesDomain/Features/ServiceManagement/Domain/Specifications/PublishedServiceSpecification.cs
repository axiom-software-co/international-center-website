using System.Linq.Expressions;
using SharedPlatform.Features.DomainPrimitives.Specifications;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Specifications;

public sealed class PublishedServiceSpecification : BaseSpecification<Service>
{
    public override Expression<Func<Service, bool>> ToExpression()
    {
        return service => service.PublishingStatus.Value == "published" && !service.IsDeleted;
    }

    public static PublishedServiceSpecification Create() => new();
}