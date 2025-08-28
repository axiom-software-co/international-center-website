using MediatR;
using CSharpFunctionalExtensions;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.CreateService;

/// <summary>
/// CQRS command for creating a new service with medical-grade audit requirements
/// Admin API command with role-based authorization and EF Core persistence
/// </summary>
public sealed record CreateServiceCommand : IRequest<Result<CreateServiceResponse>>
{
    public ServiceTitle Title { get; init; }
    public Description Description { get; init; }
    public DeliveryMode DeliveryMode { get; init; }
    public ServiceCategoryId? CategoryId { get; init; }
    public ServiceSlug? Slug { get; init; }
    public string UserId { get; init; }

    public CreateServiceCommand(
        ServiceTitle title,
        Description description,
        DeliveryMode deliveryMode,
        ServiceCategoryId? categoryId,
        ServiceSlug? slug,
        string userId)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        DeliveryMode = deliveryMode ?? throw new ArgumentNullException(nameof(deliveryMode));
        CategoryId = categoryId;
        Slug = slug;
        UserId = !string.IsNullOrWhiteSpace(userId) 
            ? userId 
            : throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
    }
}
