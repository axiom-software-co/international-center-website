using SharedPlatform.Features.DomainPrimitives.DomainEvents;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Events;

public sealed class ServicePublishedEvent : BaseDomainEvent
{
    public ServiceId ServiceId { get; init; }
    public ServiceTitle Title { get; init; }
    public Description Description { get; init; }
    public ServiceSlug Slug { get; init; }
    public DateTimeOffset PublishedAt { get; init; }

    public ServicePublishedEvent(ServiceId serviceId, ServiceTitle title, Description description, ServiceSlug slug, DateTimeOffset publishedAt)
    {
        ServiceId = serviceId ?? throw new ArgumentNullException(nameof(serviceId));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        PublishedAt = publishedAt;
    }
}