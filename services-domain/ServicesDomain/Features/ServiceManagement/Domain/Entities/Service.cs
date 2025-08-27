using SharedPlatform.Features.DomainPrimitives.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.ServiceManagement.Domain.Events;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Entities;

public sealed class Service : BaseAggregateRoot, IAuditable, ISoftDeletable, IVersioned
{
    public ServiceId ServiceId { get; private set; }
    public ServiceTitle Title { get; private set; }
    public ServiceDescription Description { get; private set; }
    public ServiceSlug Slug { get; private set; }
    public ServiceStatus Status { get; private set; }
    public ServiceCategoryId? CategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ExternalUrl { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public int Order { get; private set; }
    public bool IsFeatured { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; }

    // IAuditable
    public DateTimeOffset CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }
    
    // Legacy properties for backward compatibility
    public DateTime CreatedAt => CreatedOn.DateTime;
    public DateTime? UpdatedAt => ModifiedOn?.DateTime;

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOn { get; private set; }
    public string? DeletedBy { get; private set; }
    
    // Legacy property for backward compatibility
    public DateTime? DeletedAt => DeletedOn?.DateTime;

    // IVersioned
    public byte[] RowVersion { get; private set; }
    
    // Legacy property for backward compatibility
    public int Version { get; private set; }

    public override object Id => ServiceId.Value;

    private Service()
    {
        ServiceId = ServiceId.New();
        Title = ServiceTitle.From("Default Service");
        Description = ServiceDescription.From("Default Description");
        Slug = ServiceSlug.FromTitle(Title);
        Status = ServiceStatus.Draft;
        Metadata = new Dictionary<string, object>();
        CreatedOn = DateTimeOffset.UtcNow;
        RowVersion = new byte[8];
        Version = 1;
    }

    private Service(ServiceId id, ServiceTitle title, ServiceDescription description, ServiceSlug slug)
    {
        ServiceId = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Status = ServiceStatus.Draft;
        Order = 0;
        IsFeatured = false;
        Metadata = new Dictionary<string, object>();
        CreatedOn = DateTimeOffset.UtcNow;
        RowVersion = new byte[8];
        Version = 1;
        
        AddDomainEvent(new ServiceCreatedEvent(ServiceId, Title, Description, Slug));
    }

    public static Service Create(ServiceTitle title, ServiceDescription description, ServiceSlug? slug = null)
    {
        var serviceId = ServiceId.New();
        var serviceSlug = slug ?? ServiceSlug.FromTitle(title);
        return new Service(serviceId, title, description, serviceSlug);
    }

    public void UpdateDetails(ServiceTitle title, ServiceDescription description)
    {
        if (Status.IsDeleted || Status.IsArchived)
            throw new InvalidOperationException("Cannot update a deleted or archived service");
        
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void UpdateSlug(ServiceSlug slug)
    {
        if (Status.IsDeleted || Status.IsArchived)
            throw new InvalidOperationException("Cannot update slug of a deleted or archived service");
        
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetCategory(ServiceCategoryId categoryId)
    {
        CategoryId = categoryId;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void RemoveCategory()
    {
        CategoryId = null;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetImage(string imageUrl)
    {
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetExternalUrl(string externalUrl)
    {
        ExternalUrl = string.IsNullOrWhiteSpace(externalUrl) ? null : externalUrl.Trim();
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void Publish()
    {
        if (Status.IsDeleted || Status.IsArchived)
            throw new InvalidOperationException("Cannot publish a deleted or archived service");
        
        Status = ServiceStatus.Published;
        PublishedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ServicePublishedEvent(ServiceId, Title, Description, Slug, PublishedAt.Value));
    }

    public void Activate()
    {
        if (Status.IsDeleted || Status.IsArchived)
            throw new InvalidOperationException("Cannot activate a deleted or archived service");
        
        Status = ServiceStatus.Active;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void Deactivate()
    {
        if (Status.IsDeleted || Status.IsArchived)
            throw new InvalidOperationException("Cannot deactivate a deleted or archived service");
        
        Status = ServiceStatus.Inactive;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void Archive()
    {
        if (Status.IsDeleted)
            throw new InvalidOperationException("Cannot archive a deleted service");
        
        Status = ServiceStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ServiceArchivedEvent(ServiceId, Title, Description, Slug, ArchivedAt.Value));
    }

    public void Delete()
    {
        Status = ServiceStatus.Deleted;
        AddDomainEvent(new ServiceDeletedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));
        
        Order = order;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));
        
        Metadata[key] = value;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void RemoveMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));
        
        Metadata.Remove(key);
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }
}