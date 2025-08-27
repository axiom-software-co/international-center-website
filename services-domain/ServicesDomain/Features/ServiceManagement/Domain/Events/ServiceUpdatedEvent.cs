using SharedPlatform.Features.DomainPrimitives.DomainEvents;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Events;

public sealed class ServiceUpdatedEvent : BaseDomainEvent
{
    public ServiceId ServiceId { get; }
    public ServiceTitle Title { get; }
    public ServiceDescription Description { get; }
    public ServiceSlug Slug { get; }
    public DateTime UpdatedAt { get; }

    public ServiceUpdatedEvent(ServiceId serviceId, ServiceTitle title, ServiceDescription description, ServiceSlug slug)
        : this(serviceId, title, description, slug, DateTime.UtcNow)
    {
    }

    public ServiceUpdatedEvent(ServiceId serviceId, ServiceTitle title, ServiceDescription description, ServiceSlug slug, DateTime updatedAt)
    {
        ServiceId = serviceId;
        Title = title;
        Description = description;
        Slug = slug;
        UpdatedAt = updatedAt;
    }
}