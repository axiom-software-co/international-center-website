using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class ServiceDescription : BaseValueObject
{
    public const int MaxLength = 2000;
    public const int MinLength = 10;
    
    public string Value { get; private set; }

    private ServiceDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ServiceDescription cannot be null or empty", nameof(value));
        
        if (value.Length < MinLength)
            throw new ArgumentException($"ServiceDescription must be at least {MinLength} characters", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"ServiceDescription cannot exceed {MaxLength} characters", nameof(value));
        
        Value = value.Trim();
    }

    public static ServiceDescription From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(ServiceDescription description) => description.Value;
    public static implicit operator ServiceDescription(string value) => From(value);
    
    public override string ToString() => Value;
}