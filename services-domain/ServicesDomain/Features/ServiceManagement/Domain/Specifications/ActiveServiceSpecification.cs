using System.Linq.Expressions;
using SharedPlatform.Features.DomainPrimitives.Specifications;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Specifications;

public sealed class ActiveServiceSpecification : BaseSpecification<Service>
{
    public override Expression<Func<Service, bool>> ToExpression()
    {
        return service => service.Status.Value == ServiceStatusType.Active;
    }

    public static ActiveServiceSpecification Create() => new();
}