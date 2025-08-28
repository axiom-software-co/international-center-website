using SharedPlatform.Features.DomainPrimitives.Entities;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.CategoryManagement.Domain.Entities;

/// <summary>
/// Featured Category entity - represents categories featured in prominent positions
/// Matches featured_categories table from SERVICES-SCHEMA.md exactly
/// Only positions 1 and 2 are allowed, with unique constraints
/// </summary>
public sealed class FeaturedCategory : BaseEntity
{
    public Guid FeaturedCategoryId { get; private set; }
    public ServiceCategoryId CategoryId { get; private set; }
    public int FeaturePosition { get; private set; } // 1 or 2 only
    
    // Audit fields
    public DateTimeOffset CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }
    
    public override object Id => FeaturedCategoryId;
    
    private FeaturedCategory()
    {
        // EF Core constructor
        FeaturedCategoryId = Guid.NewGuid();
        CategoryId = ServiceCategoryId.New();
        FeaturePosition = 1;
        CreatedOn = DateTimeOffset.UtcNow;
    }
    
    private FeaturedCategory(ServiceCategoryId categoryId, int featurePosition, string? createdBy = null)
    {
        FeaturedCategoryId = Guid.NewGuid();
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        FeaturePosition = ValidateFeaturePosition(featurePosition);
        CreatedOn = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
    }
    
    /// <summary>
    /// Creates a new featured category for the specified position
    /// </summary>
    public static FeaturedCategory Create(ServiceCategoryId categoryId, int featurePosition, string? createdBy = null)
    {
        return new FeaturedCategory(categoryId, featurePosition, createdBy);
    }
    
    /// <summary>
    /// Updates the feature position (must be 1 or 2)
    /// </summary>
    public void UpdatePosition(int newPosition, string? modifiedBy = null)
    {
        FeaturePosition = ValidateFeaturePosition(newPosition);
        ModifiedOn = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    /// <summary>
    /// Validates that feature position is 1 or 2 only (per database constraint)
    /// </summary>
    private static int ValidateFeaturePosition(int position)
    {
        if (position is not (1 or 2))
        {
            throw new ArgumentException("Feature position must be 1 or 2", nameof(position));
        }
        
        return position;
    }
    
    /// <summary>
    /// Business rule: Cannot feature the default unassigned category
    /// This will be implemented when ServiceCategory entity is complete
    /// </summary>
    public static void ValidateNotDefaultUnassigned(ServiceCategory category)
    {
        // TODO: Implement when ServiceCategory.IsDefaultUnassigned is available
        // if (category.IsDefaultUnassigned)
        // {
        //     throw new InvalidOperationException("Cannot feature the default unassigned category");
        // }
    }
}