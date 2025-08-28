using ServicesDomain.Features.ServiceManagement.Domain.Entities;

namespace ServicesDomain.Features.CreateService;

/// <summary>
/// Response DTO for CreateService admin API operation
/// Includes audit information for medical-grade compliance tracking
/// </summary>
public sealed record CreateServiceResponse
{
    public Guid ServiceId { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string Slug { get; init; }
    public string DeliveryMode { get; init; }
    public Guid CategoryId { get; init; }
    public string? ImageUrl { get; init; }
    public string? LongDescriptionUrl { get; init; }
    public int OrderNumber { get; init; }
    public string PublishingStatus { get; init; }
    
    // Audit fields for medical compliance
    public DateTimeOffset CreatedOn { get; init; }
    public string? CreatedBy { get; init; }

    private CreateServiceResponse(
        Guid serviceId,
        string title,
        string description,
        string slug,
        string deliveryMode,
        Guid categoryId,
        string? imageUrl,
        string? longDescriptionUrl,
        int orderNumber,
        string publishingStatus,
        DateTimeOffset createdOn,
        string? createdBy)
    {
        ServiceId = serviceId;
        Title = title;
        Description = description;
        Slug = slug;
        DeliveryMode = deliveryMode;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        LongDescriptionUrl = longDescriptionUrl;
        OrderNumber = orderNumber;
        PublishingStatus = publishingStatus;
        CreatedOn = createdOn;
        CreatedBy = createdBy;
    }

    public static CreateServiceResponse From(Service service)
    {
        return new CreateServiceResponse(
            service.ServiceId.Value,
            service.Title.Value,
            service.Description.Value,
            service.Slug.Value,
            service.DeliveryMode.Value,
            service.CategoryId.Value,
            service.ImageUrl,
            service.LongDescriptionUrl?.ToString(),
            service.OrderNumber,
            service.PublishingStatus.Value,
            service.CreatedOn,
            service.CreatedBy);
    }
}
