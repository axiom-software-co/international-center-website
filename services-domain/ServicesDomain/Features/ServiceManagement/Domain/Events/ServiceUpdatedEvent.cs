using SharedPlatform.Features.DomainPrimitives.DomainEvents;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Events;

public sealed class ServiceUpdatedEvent : BaseDomainEvent
{
    public ServiceId ServiceId { get; }
    public ServiceTitle Title { get; }
    public Description Description { get; }
    public ServiceSlug Slug { get; }
    public DateTimeOffset UpdatedAt { get; }

    public ServiceUpdatedEvent(ServiceId serviceId, ServiceTitle title, Description description, ServiceSlug slug)
        : this(serviceId, title, description, slug, DateTimeOffset.UtcNow)
    {
    }

    public ServiceUpdatedEvent(ServiceId serviceId, ServiceTitle title, Description description, ServiceSlug slug, DateTimeOffset updatedAt)
    {
        ServiceId = serviceId;
        Title = title;
        Description = description;
        Slug = slug;
        UpdatedAt = updatedAt;
    }
}