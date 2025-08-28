using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.GetService;

/// <summary>
/// Query object for retrieving a service by its unique identifier
/// Follows CQRS pattern for read operations with validation support
/// </summary>
public sealed record GetServiceQuery
{
    public ServiceId ServiceId { get; init; }
    
    public GetServiceQuery(ServiceId serviceId)
    {
        ServiceId = serviceId ?? throw new ArgumentNullException(nameof(serviceId));
    }
    
    /// <summary>
    /// Creates a query from a GUID - convenience method for API controllers
    /// </summary>
    public static GetServiceQuery FromGuid(Guid serviceId)
    {
        return new GetServiceQuery(ServiceId.From(serviceId));
    }
    
    /// <summary>
    /// Creates a query from a string - convenience method for API controllers
    /// </summary>
    public static GetServiceQuery FromString(string serviceId)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
            throw new ArgumentException("Service ID cannot be null or empty", nameof(serviceId));
            
        if (!Guid.TryParse(serviceId, out var guid))
            throw new ArgumentException("Service ID must be a valid GUID", nameof(serviceId));
            
        return FromGuid(guid);
    }
}
