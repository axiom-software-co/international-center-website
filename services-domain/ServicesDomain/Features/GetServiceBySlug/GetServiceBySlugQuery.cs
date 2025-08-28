using MediatR;
using CSharpFunctionalExtensions;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.GetService;

namespace ServicesDomain.Features.GetServiceBySlug;

/// <summary>
/// CQRS query for retrieving a service by its URL-friendly slug
/// Optimized for public API access with caching and medical-grade audit requirements
/// </summary>
public sealed record GetServiceBySlugQuery : IRequest<Result<GetServiceResponse>>
{
    public ServiceSlug ServiceSlug { get; init; }

    public GetServiceBySlugQuery(ServiceSlug serviceSlug)
    {
        ServiceSlug = serviceSlug ?? throw new ArgumentNullException(nameof(serviceSlug));
    }
}
