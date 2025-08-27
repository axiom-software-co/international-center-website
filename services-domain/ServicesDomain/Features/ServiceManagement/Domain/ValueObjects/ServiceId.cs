using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class ServiceId : BaseValueObject
{
    public Guid Value { get; private set; }

    private ServiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceId cannot be empty", nameof(value));
        
        Value = value;
    }

    public static ServiceId New() => new(Guid.NewGuid());
    
    public static ServiceId From(Guid value) => new(value);
    
    public static ServiceId From(string value)
    {
        if (Guid.TryParse(value, out var guid))
            return new ServiceId(guid);
        
        throw new ArgumentException($"Invalid ServiceId format: {value}", nameof(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(ServiceId serviceId) => serviceId.Value;
    public static implicit operator ServiceId(Guid guid) => From(guid);
    
    public override string ToString() => Value.ToString();
}