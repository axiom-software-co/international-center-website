using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class DeliveryMode : BaseValueObject
{
    public static readonly string[] ValidValues = { "mobile_service", "outpatient_service", "inpatient_service" };
    
    public string Value { get; private set; }

    private DeliveryMode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DeliveryMode cannot be null or empty", nameof(value));
        
        var normalizedValue = value.Trim().ToLowerInvariant();
        if (!ValidValues.Contains(normalizedValue))
            throw new ArgumentException($"Invalid DeliveryMode: {value}. Valid values are: {string.Join(", ", ValidValues)}", nameof(value));
        
        Value = normalizedValue;
    }

    public static DeliveryMode MobileService => new("mobile_service");
    public static DeliveryMode OutpatientService => new("outpatient_service");
    public static DeliveryMode InpatientService => new("inpatient_service");
    
    public static DeliveryMode From(string value) => new(value);

    public bool IsMobileService => Value == "mobile_service";
    public bool IsOutpatientService => Value == "outpatient_service";
    public bool IsInpatientService => Value == "inpatient_service";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(DeliveryMode deliveryMode) => deliveryMode.Value;
    public static implicit operator DeliveryMode(string value) => From(value);
    
    public override string ToString() => Value;
}