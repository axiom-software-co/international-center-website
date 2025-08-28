namespace SharedPlatform.Features.DataAccess.Abstractions;

/// <summary>
/// Service domain contract for data access infrastructure
/// RED PHASE: Abstract contract to avoid circular dependency with ServicesDomain
/// ServicesDomain will implement this interface through adapter pattern
/// </summary>
public interface IService
{
    Guid ServiceId { get; }
    string Title { get; }
    string Description { get; }
    string Slug { get; }
    string DeliveryMode { get; }
    Guid CategoryId { get; }
    string CreatedBy { get; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
    
    // Content management properties
    string? ContentUrl { get; set; }
    string? ContentHash { get; set; }
    DateTimeOffset? LastContentUpdate { get; set; }
}

/// <summary>
/// Service ID value object contract
/// RED PHASE: Abstract contract for strongly-typed service identifier
/// </summary>
public interface IServiceId
{
    Guid Value { get; }
}

/// <summary>
/// Service slug value object contract
/// RED PHASE: Abstract contract for URL-friendly service identifier
/// </summary>
public interface IServiceSlug
{
    string Value { get; }
}

/// <summary>
/// Service category ID value object contract
/// RED PHASE: Abstract contract for service categorization
/// </summary>
public interface IServiceCategoryId
{
    Guid Value { get; }
}