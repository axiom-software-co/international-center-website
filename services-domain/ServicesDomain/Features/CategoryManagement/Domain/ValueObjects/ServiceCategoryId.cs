using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

public sealed class ServiceCategoryId : BaseValueObject
{
    public Guid Value { get; private set; }

    private ServiceCategoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceCategoryId cannot be empty", nameof(value));
        
        Value = value;
    }

    public static ServiceCategoryId New() => new(Guid.NewGuid());
    
    public static ServiceCategoryId From(Guid value) => new(value);
    
    public static ServiceCategoryId From(string value)
    {
        if (Guid.TryParse(value, out var guid))
            return new ServiceCategoryId(guid);
        
        throw new ArgumentException($"Invalid ServiceCategoryId format: {value}", nameof(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(ServiceCategoryId categoryId) => categoryId.Value;
    public static implicit operator ServiceCategoryId(Guid guid) => From(guid);
    
    public override string ToString() => Value.ToString();
}