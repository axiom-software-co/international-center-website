using SharedPlatform.Features.DomainPrimitives.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Entities;

public sealed class ServiceMetadata : BaseEntity
{
    public ServiceId ServiceId { get; private set; }
    public string Key { get; private set; }
    public string Value { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    private Guid _id;
    public override object Id => _id;

    private ServiceMetadata() 
    {
        _id = Guid.NewGuid();
        ServiceId = ServiceId.New(); // Provide default value
        Key = string.Empty;
        Value = string.Empty;
    }

    private ServiceMetadata(ServiceId serviceId, string key, string value, string? description = null)
    {
        _id = Guid.NewGuid();
        ServiceId = serviceId ?? throw new ArgumentNullException(nameof(serviceId));
        Key = !string.IsNullOrWhiteSpace(key) ? key.Trim() : throw new ArgumentException("Key cannot be null or empty", nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public static ServiceMetadata Create(ServiceId serviceId, string key, string value, string? description = null)
    {
        return new ServiceMetadata(serviceId, key, value, description);
    }

    public void UpdateValue(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}