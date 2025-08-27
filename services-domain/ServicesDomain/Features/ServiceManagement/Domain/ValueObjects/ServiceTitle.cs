using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class ServiceTitle : BaseValueObject
{
    public const int MaxLength = 200;
    public const int MinLength = 3;
    
    public string Value { get; private set; }

    private ServiceTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ServiceTitle cannot be null or empty", nameof(value));
        
        if (value.Length < MinLength)
            throw new ArgumentException($"ServiceTitle must be at least {MinLength} characters", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"ServiceTitle cannot exceed {MaxLength} characters", nameof(value));
        
        Value = value.Trim();
    }

    public static ServiceTitle From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(ServiceTitle title) => title.Value;
    public static implicit operator ServiceTitle(string value) => From(value);
    
    public override string ToString() => Value;
}