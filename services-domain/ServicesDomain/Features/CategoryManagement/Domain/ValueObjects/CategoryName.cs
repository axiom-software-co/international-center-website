using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

public sealed class CategoryName : BaseValueObject
{
    public const int MaxLength = 100;
    public const int MinLength = 2;
    
    public string Value { get; private set; }

    private CategoryName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CategoryName cannot be null or empty", nameof(value));
        
        if (value.Length < MinLength)
            throw new ArgumentException($"CategoryName must be at least {MinLength} characters", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"CategoryName cannot exceed {MaxLength} characters", nameof(value));
        
        Value = value.Trim();
    }

    public static CategoryName From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(CategoryName name) => name.Value;
    public static implicit operator CategoryName(string value) => From(value);
    
    public override string ToString() => Value;
}