using SharedPlatform.Features.DomainPrimitives.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.ServiceManagement.Domain.Events;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Entities;

public sealed class Service : BaseAggregateRoot, IAuditable, ISoftDeletable, IVersioned
{
    public ServiceId ServiceId { get; private set; }
    public ServiceTitle Title { get; private set; }
    public Description Description { get; private set; }
    public ServiceSlug Slug { get; private set; }
    public LongDescriptionUrl? LongDescriptionUrl { get; private set; }
    public ServiceCategoryId CategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public int OrderNumber { get; private set; }
    public DeliveryMode DeliveryMode { get; private set; }
    public PublishingStatus PublishingStatus { get; private set; }

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
        Description = Description.From("Default description for compilation");
        Slug = ServiceSlug.FromTitle(Title);
        PublishingStatus = PublishingStatus.Draft;
        DeliveryMode = DeliveryMode.OutpatientService;
        CategoryId = ServiceCategoryId.New(); // Will be assigned to default category
        OrderNumber = 0;
        CreatedOn = DateTimeOffset.UtcNow;
        RowVersion = new byte[8];
        Version = 1;
    }

    private Service(ServiceId id, ServiceTitle title, Description description, ServiceSlug slug, DeliveryMode deliveryMode, ServiceCategoryId categoryId)
    {
        ServiceId = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        DeliveryMode = deliveryMode ?? throw new ArgumentNullException(nameof(deliveryMode));
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        PublishingStatus = PublishingStatus.Draft;
        OrderNumber = 0;
        CreatedOn = DateTimeOffset.UtcNow;
        RowVersion = new byte[8];
        Version = 1;
        
        AddDomainEvent(new ServiceCreatedEvent(ServiceId, Title, Description, Slug));
    }

    public static Service Create(ServiceTitle title, Description description, DeliveryMode deliveryMode, ServiceCategoryId categoryId, ServiceSlug? slug = null)
    {
        var serviceId = ServiceId.New();
        var serviceSlug = slug ?? ServiceSlug.FromTitle(title);
        return new Service(serviceId, title, description, serviceSlug, deliveryMode, categoryId);
    }

    public void UpdateDetails(ServiceTitle title, Description description)
    {
        if (IsDeleted || PublishingStatus.IsArchived)
            throw new InvalidOperationException("Cannot update a deleted or archived service");
        
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        ModifiedOn = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void UpdateSlug(ServiceSlug slug)
    {
        if (IsDeleted || PublishingStatus.IsArchived)
            throw new InvalidOperationException("Cannot update slug of a deleted or archived service");
        
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        ModifiedOn = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetCategory(ServiceCategoryId categoryId)
    {
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetImage(string? imageUrl)
    {
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetLongDescriptionUrl(LongDescriptionUrl? longDescriptionUrl)
    {
        LongDescriptionUrl = longDescriptionUrl;
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetDeliveryMode(DeliveryMode deliveryMode)
    {
        DeliveryMode = deliveryMode ?? throw new ArgumentNullException(nameof(deliveryMode));
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }

    public void Publish()
    {
        if (IsDeleted || PublishingStatus.IsArchived)
            throw new InvalidOperationException("Cannot publish a deleted or archived service");
        
        PublishingStatus = PublishingStatus.Published;
        ModifiedOn = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new ServicePublishedEvent(ServiceId, Title, Description, Slug, DateTimeOffset.UtcNow));
    }

    public void Archive()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot archive a deleted service");
        
        PublishingStatus = PublishingStatus.Archived;
        ModifiedOn = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new ServiceArchivedEvent(ServiceId, Title, Description, Slug, DateTimeOffset.UtcNow));
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedOn = DateTimeOffset.UtcNow;
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new ServiceDeletedEvent(ServiceId, Title, Description, Slug));
    }
    
    public void Delete(string deletedBy, DateTimeOffset deletedOn)
    {
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be null or empty for audit compliance", nameof(deletedBy));
            
        IsDeleted = true;
        DeletedOn = deletedOn;
        DeletedBy = deletedBy;
        ModifiedOn = deletedOn;
        ModifiedBy = deletedBy;
        AddDomainEvent(new ServiceDeletedEvent(ServiceId, Title, Description, Slug));
    }

    public void SetOrderNumber(int orderNumber)
    {
        if (orderNumber < 0)
            throw new ArgumentException("Order number cannot be negative", nameof(orderNumber));
        
        OrderNumber = orderNumber;
        ModifiedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new ServiceUpdatedEvent(ServiceId, Title, Description, Slug));
    }
}