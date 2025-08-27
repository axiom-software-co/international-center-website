using SharedPlatform.Features.DomainPrimitives.DomainEvents;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Events;

public sealed class ServiceDeletedEvent : BaseDomainEvent
{
    public ServiceId ServiceId { get; }
    public ServiceTitle Title { get; }
    public ServiceDescription Description { get; }
    public ServiceSlug Slug { get; }
    public DateTime DeletedAt { get; }

    public ServiceDeletedEvent(ServiceId serviceId, ServiceTitle title, ServiceDescription description, ServiceSlug slug)
        : this(serviceId, title, description, slug, DateTime.UtcNow)
    {
    }

    public ServiceDeletedEvent(ServiceId serviceId, ServiceTitle title, ServiceDescription description, ServiceSlug slug, DateTime deletedAt)
    {
        ServiceId = serviceId;
        Title = title;
        Description = description;
        Slug = slug;
        DeletedAt = deletedAt;
    }
}