using SharedPlatform.Features.ContentManagement.Abstractions;

namespace SharedPlatform.Features.DataAccess.Abstractions;

/// <summary>
/// Service data access contract for content management operations
/// RED PHASE: Abstract contract to support content management infrastructure
/// </summary>
public interface IServiceDataAccess
{
    Task<IService> AddAsync(IService service);
    Task<IService?> GetByIdAsync(Guid serviceId);
    Task<IService?> GetBySlugAsync(string slug);
    Task UpdateAsync(IService service);
    Task<IEnumerable<IService>> GetServicesWithoutContentAsync();
    Task<IEnumerable<IService>> GetServicesWithExpiredContentAsync(TimeSpan maxAge);
}

/// <summary>
/// Audit data access contract for content management compliance
/// RED PHASE: Abstract contract to support audit logging infrastructure
/// </summary>
public interface IAuditDataAccess
{
    Task CreateContentAuditAsync(ContentAuditEntry entry);
    Task CreateAuditContextAsync(ContentAuditContext context);
    Task<IEnumerable<ContentAuditEntry>> GetAuditEntriesAsync(string serviceId, DateTimeOffset? from = null, DateTimeOffset? to = null);
}