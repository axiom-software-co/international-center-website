using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.GetService;

/// <summary>
/// Response DTO for GetService query containing service details
/// Optimized for JSON serialization and API consumption
/// </summary>
public sealed record GetServiceResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LongDescriptionUrl { get; init; }
    public Guid CategoryId { get; init; }
    public string? ImageUrl { get; init; }
    public int OrderNumber { get; init; }
    public string DeliveryMode { get; init; } = string.Empty;
    public string PublishingStatus { get; init; } = string.Empty;
    public DateTimeOffset CreatedOn { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset? ModifiedOn { get; init; }
    public string? ModifiedBy { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset? DeletedOn { get; init; }
    public string? DeletedBy { get; init; }
    
    /// <summary>
    /// Maps a Service domain entity to a GetServiceResponse DTO
    /// </summary>
    public static GetServiceResponse From(Service service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
            
        return new GetServiceResponse
        {
            Id = service.ServiceId.Value,
            Title = service.Title.Value,
            Description = service.Description.Value,
            Slug = service.Slug.Value,
            LongDescriptionUrl = service.LongDescriptionUrl?.Value,
            CategoryId = service.CategoryId.Value,
            ImageUrl = service.ImageUrl,
            OrderNumber = service.OrderNumber,
            DeliveryMode = service.DeliveryMode.Value,
            PublishingStatus = service.PublishingStatus.Value,
            CreatedOn = service.CreatedOn,
            CreatedBy = service.CreatedBy,
            ModifiedOn = service.ModifiedOn,
            ModifiedBy = service.ModifiedBy,
            IsDeleted = service.IsDeleted,
            DeletedOn = service.DeletedOn,
            DeletedBy = service.DeletedBy
        };
    }
    
    /// <summary>
    /// Creates a minimal response for API listing purposes
    /// </summary>
    public static GetServiceResponse FromMinimal(Service service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
            
        return new GetServiceResponse
        {
            Id = service.ServiceId.Value,
            Title = service.Title.Value,
            Description = service.Description.Value,
            Slug = service.Slug.Value,
            DeliveryMode = service.DeliveryMode.Value,
            PublishingStatus = service.PublishingStatus.Value,
            CreatedOn = service.CreatedOn,
            IsDeleted = service.IsDeleted
        };
    }
}
