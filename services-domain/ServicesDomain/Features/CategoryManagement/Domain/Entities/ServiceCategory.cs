using SharedPlatform.Features.DomainPrimitives.Entities;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.CategoryManagement.Domain.Entities;

/// <summary>
/// Service Category entity - represents categories for organizing services
/// Matches service_categories table from SERVICES-SCHEMA.md exactly
/// Supports soft delete and unique default unassigned category
/// </summary>
public sealed class ServiceCategory : BaseEntity, IAuditable, ISoftDeletable
{
    public ServiceCategoryId CategoryId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public int OrderNumber { get; private set; }
    public bool IsDefaultUnassigned { get; private set; }
    
    // IAuditable implementation
    public DateTimeOffset CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }
    
    // ISoftDeletable implementation
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOn { get; private set; }
    public string? DeletedBy { get; private set; }
    
    public override object Id => CategoryId.Value;
    
    private ServiceCategory()
    {
        // EF Core constructor
        CategoryId = ServiceCategoryId.New();
        Name = string.Empty;
        Slug = string.Empty;
        OrderNumber = 0;
        IsDefaultUnassigned = false;
        IsDeleted = false;
    }
    
    private ServiceCategory(string name, string slug, int orderNumber = 0, bool isDefaultUnassigned = false, string? createdBy = null)
    {
        CategoryId = ServiceCategoryId.New();
        Name = ValidateName(name);
        Slug = ValidateSlug(slug);
        OrderNumber = orderNumber;
        IsDefaultUnassigned = isDefaultUnassigned;
        IsDeleted = false;
        CreatedOn = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
    }
    
    /// <summary>
    /// Creates a new service category
    /// </summary>
    public static ServiceCategory Create(string name, string slug, int orderNumber = 0, string? createdBy = null)
    {
        return new ServiceCategory(name, slug, orderNumber, false, createdBy);
    }
    
    /// <summary>
    /// Creates the default unassigned category (only one allowed system-wide)
    /// </summary>
    public static ServiceCategory CreateDefaultUnassigned(string? createdBy = null)
    {
        return new ServiceCategory("Unassigned", "unassigned", 0, true, createdBy);
    }
    
    /// <summary>
    /// Updates category details
    /// </summary>
    public void Update(string name, string slug, int orderNumber, string? modifiedBy = null)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update a deleted category");
            
        Name = ValidateName(name);
        Slug = ValidateSlug(slug);
        OrderNumber = orderNumber;
        ModifiedOn = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    /// <summary>
    /// Soft delete the category
    /// </summary>
    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted)
            return; // Already deleted
            
        if (IsDefaultUnassigned)
            throw new InvalidOperationException("Cannot delete the default unassigned category");
            
        IsDeleted = true;
        DeletedOn = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        ModifiedOn = DateTimeOffset.UtcNow;
        ModifiedBy = deletedBy;
    }
    
    /// <summary>
    /// Restore a soft-deleted category
    /// </summary>
    public void Restore(string? modifiedBy = null)
    {
        if (!IsDeleted)
            return; // Not deleted
            
        IsDeleted = false;
        DeletedOn = null;
        DeletedBy = null;
        ModifiedOn = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    /// <summary>
    /// Updates the order number for sorting
    /// </summary>
    public void UpdateOrder(int newOrderNumber, string? modifiedBy = null)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update order of a deleted category");
            
        OrderNumber = newOrderNumber;
        ModifiedOn = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
    
    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));
            
        if (name.Length > 255)
            throw new ArgumentException("Category name cannot exceed 255 characters", nameof(name));
            
        return name.Trim();
    }
    
    private static string ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Category slug cannot be null or empty", nameof(slug));
            
        if (slug.Length > 255)
            throw new ArgumentException("Category slug cannot exceed 255 characters", nameof(slug));
            
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        
        // Basic slug validation - alphanumeric and hyphens only
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedSlug, @"^[a-z0-9-]+$"))
            throw new ArgumentException("Category slug must contain only lowercase letters, numbers, and hyphens", nameof(slug));
            
        return normalizedSlug;
    }
}
