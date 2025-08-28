using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class Description : BaseValueObject
{
    public const int MaxLength = 500;
    public const int MinLength = 10;
    
    public string Value { get; private set; }

    private Description(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Description cannot be null or empty", nameof(value));
        
        if (value.Length < MinLength)
            throw new ArgumentException($"Description must be at least {MinLength} characters", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"Description cannot exceed {MaxLength} characters", nameof(value));
        
        Value = value.Trim();
    }

    public static Description From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Description description) => description.Value;
    public static implicit operator Description(string value) => From(value);
    
    public override string ToString() => Value;
}