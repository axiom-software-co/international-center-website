using SharedPlatform.Features.DomainPrimitives.DomainEvents;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Events;

public sealed class ServiceCreatedEvent : BaseDomainEvent
{
    public ServiceId ServiceId { get; }
    public ServiceTitle Title { get; }
    public ServiceDescription Description { get; }
    public ServiceSlug Slug { get; }
    public DateTime CreatedAt { get; }

    public ServiceCreatedEvent(ServiceId serviceId, ServiceTitle title, ServiceDescription description, ServiceSlug slug)
        : this(serviceId, title, description, slug, DateTime.UtcNow)
    {
    }

    public ServiceCreatedEvent(ServiceId serviceId, ServiceTitle title, ServiceDescription description, ServiceSlug slug, DateTime createdAt)
    {
        ServiceId = serviceId;
        Title = title;
        Description = description;
        Slug = slug;
        CreatedAt = createdAt;
    }
}