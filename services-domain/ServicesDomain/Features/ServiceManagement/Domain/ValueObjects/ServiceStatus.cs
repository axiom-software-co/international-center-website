using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public enum ServiceStatusType
{
    Draft = 0,
    Active = 1,
    Inactive = 2,
    Published = 3,
    Archived = 4,
    Deleted = 5
}

public sealed class ServiceStatus : BaseValueObject
{
    public ServiceStatusType Value { get; private set; }

    private ServiceStatus(ServiceStatusType value)
    {
        Value = value;
    }

    public static ServiceStatus Draft => new(ServiceStatusType.Draft);
    public static ServiceStatus Active => new(ServiceStatusType.Active);
    public static ServiceStatus Inactive => new(ServiceStatusType.Inactive);
    public static ServiceStatus Published => new(ServiceStatusType.Published);
    public static ServiceStatus Archived => new(ServiceStatusType.Archived);
    public static ServiceStatus Deleted => new(ServiceStatusType.Deleted);
    
    public static ServiceStatus From(ServiceStatusType value) => new(value);
    public static ServiceStatus From(string value)
    {
        if (Enum.TryParse<ServiceStatusType>(value, true, out var status))
            return new ServiceStatus(status);
        
        throw new ArgumentException($"Invalid ServiceStatus: {value}", nameof(value));
    }

    public bool IsActive => Value == ServiceStatusType.Active;
    public bool IsPublished => Value == ServiceStatusType.Published;
    public bool IsDeleted => Value == ServiceStatusType.Deleted;
    public bool IsArchived => Value == ServiceStatusType.Archived;
    public bool IsDraft => Value == ServiceStatusType.Draft;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator ServiceStatusType(ServiceStatus status) => status.Value;
    public static implicit operator ServiceStatus(ServiceStatusType value) => From(value);
    
    public override string ToString() => Value.ToString();
}