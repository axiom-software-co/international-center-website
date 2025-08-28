using SharedPlatform.Features.ContentManagement.Abstractions;
using SharedPlatform.Features.Caching.Abstractions;

namespace SharedPlatform.Features.DataAccess.Abstractions;

/// <summary>
/// Test implementations of domain contracts for RED phase testing
/// These will be replaced by actual ServicesDomain implementations in GREEN phase
/// </summary>
internal sealed class TestService : IService
{
    public Guid ServiceId { get; init; } = Guid.NewGuid();
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string DeliveryMode { get; init; } = string.Empty;
    public Guid CategoryId { get; init; } = Guid.NewGuid();
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    
    // Content management properties
    public string? ContentUrl { get; set; }
    public string? ContentHash { get; set; }
    public DateTimeOffset? LastContentUpdate { get; set; }
}

internal sealed class TestServiceId : IServiceId
{
    public Guid Value { get; }

    public TestServiceId(Guid value) => Value = value;

    public static TestServiceId New() => new(Guid.NewGuid());
}

internal sealed class TestServiceSlug : IServiceSlug
{
    public string Value { get; }

    public TestServiceSlug(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));

    public static TestServiceSlug From(string value) => new(value);
}

internal sealed class TestServiceCategoryId : IServiceCategoryId
{
    public Guid Value { get; }

    public TestServiceCategoryId(Guid value) => Value = value;

    public static TestServiceCategoryId New() => new(Guid.NewGuid());
}

/// <summary>
/// Test implementation of service data access for RED phase testing
/// </summary>
public sealed class TestServiceDataAccess : IServiceDataAccess
{
    private readonly Dictionary<Guid, IService> _services = new();

    public TestServiceDataAccess()
    {
        // Initialize with test data
        var testService = new TestService();
        _services[testService.ServiceId] = testService;
    }

    public Task<IService> AddAsync(IService service)
    {
        _services[service.ServiceId] = service;
        return Task.FromResult(service);
    }

    public Task<IService?> GetByIdAsync(Guid serviceId)
    {
        _services.TryGetValue(serviceId, out var service);
        return Task.FromResult(service);
    }

    public Task<IService?> GetBySlugAsync(string slug)
    {
        var service = _services.Values.FirstOrDefault(s => s.Slug == slug);
        return Task.FromResult(service);
    }

    public Task UpdateAsync(IService service)
    {
        if (service is TestService testService)
        {
            _services[testService.ServiceId] = testService;
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IService>> GetServicesWithoutContentAsync()
    {
        var services = _services.Values.Where(s => string.IsNullOrEmpty(s.ContentUrl));
        return Task.FromResult(services);
    }

    public Task<IEnumerable<IService>> GetServicesWithExpiredContentAsync(TimeSpan maxAge)
    {
        var cutoff = DateTimeOffset.UtcNow - maxAge;
        var services = _services.Values.Where(s => 
            s.LastContentUpdate.HasValue && s.LastContentUpdate.Value < cutoff);
        return Task.FromResult(services);
    }
}

/// <summary>
/// Test implementation of audit data access for RED phase testing
/// </summary>
public sealed class TestAuditDataAccess : IAuditDataAccess
{
    private readonly List<ContentAuditEntry> _auditEntries = new();
    private readonly List<ContentAuditContext> _auditContexts = new();

    public Task CreateContentAuditAsync(ContentAuditEntry entry)
    {
        _auditEntries.Add(entry);
        return Task.CompletedTask;
    }

    public Task CreateAuditContextAsync(ContentAuditContext context)
    {
        _auditContexts.Add(context);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ContentAuditEntry>> GetAuditEntriesAsync(string serviceId, DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        if (!Guid.TryParse(serviceId, out var serviceGuid))
            return Task.FromResult<IEnumerable<ContentAuditEntry>>([]);

        var entries = _auditEntries.Where(e => e.ServiceId == serviceGuid);
        
        if (from.HasValue)
            entries = entries.Where(e => e.AuditTimestamp >= from.Value);
        
        if (to.HasValue)
            entries = entries.Where(e => e.AuditTimestamp <= to.Value);
        
        return Task.FromResult(entries);
    }
}

/// <summary>
/// Test implementation of cache service for testing caching behavior
/// </summary>
public sealed class TestCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime ExpiresAt)> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if entry has expired
            if (DateTime.UtcNow <= entry.ExpiresAt)
            {
                return Task.FromResult(entry.Value as T);
            }
            else
            {
                // Remove expired entry
                _cache.Remove(key);
            }
        }
        
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var expiresAt = expiration.HasValue 
            ? DateTime.UtcNow.Add(expiration.Value) 
            : DateTime.UtcNow.AddHours(1); // Default 1 hour expiration
        
        _cache[key] = (value, expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if entry has expired
            if (DateTime.UtcNow <= entry.ExpiresAt)
            {
                return Task.FromResult(true);
            }
            else
            {
                // Remove expired entry
                _cache.Remove(key);
            }
        }
        
        return Task.FromResult(false);
    }
}