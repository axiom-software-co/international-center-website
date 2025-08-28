using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using SharedPlatform.Features.DataAccess.Abstractions;

namespace SharedPlatform.Features.DataAccess.Dapper;

/// <summary>
/// High-performance Dapper repository for read-heavy operations (Public APIs)
/// REFACTOR PHASE: Optimized implementation with connection factory integration
/// Features automatic connection management, query caching, and performance monitoring
/// Medical-grade error handling with connection pooling and structured logging
/// </summary>
public sealed class DapperServiceRepository
{
    private readonly DapperConnectionFactory _connectionFactory;
    private readonly ILogger<DapperServiceRepository> _logger;

    // High-performance LoggerMessage delegates for medical-grade logging
    private static readonly Action<ILogger, string, long, Exception?> LogQueryExecuted =
        LoggerMessage.Define<string, long>(LogLevel.Debug, new EventId(3001, nameof(LogQueryExecuted)),
            "Dapper query executed: {QueryName} in {Duration}ms");

    private static readonly Action<ILogger, string, long, Exception> LogQueryFailed =
        LoggerMessage.Define<string, long>(LogLevel.Error, new EventId(3002, nameof(LogQueryFailed)),
            "Dapper query failed: {QueryName} - Duration: {Duration}ms");

    public DapperServiceRepository(
        DapperConnectionFactory connectionFactory,
        ILogger<DapperServiceRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    private sealed class ServiceDto : IService
    {
        public Guid ServiceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string DeliveryMode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Content management properties
        public string? ContentUrl { get; set; }
        public string? ContentHash { get; set; }
        public DateTimeOffset? LastContentUpdate { get; set; }
        
        // Map from database column names to interface properties
        public DateTime CreatedOn { set => CreatedAt = value; }
        public DateTime ModifiedOn { set => UpdatedAt = value; }
    }

    public async Task<IEnumerable<IService>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: High-performance query with connection management and monitoring
        using var activity = new Activity("Dapper.GetAllServices");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            
            const string sql = @"
                SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                       slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                       created_by AS CreatedBy, created_on AS CreatedOn, modified_on AS ModifiedOn
                FROM services 
                WHERE is_deleted = false
                ORDER BY created_on DESC";

            var services = await connection.QueryAsync<ServiceDto>(sql).ConfigureAwait(false);
            
            LogQueryExecuted(_logger, nameof(GetAllAsync), stopwatch.ElapsedMilliseconds, null);
            return services;
        }
        catch (Exception ex)
        {
            LogQueryFailed(_logger, nameof(GetAllAsync), stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<IService?> GetByIdAsync(IServiceId serviceId, CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: High-performance single record query with monitoring
        using var activity = new Activity("Dapper.GetServiceById");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            
            const string sql = @"
                SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                       slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                       created_by AS CreatedBy, created_on AS CreatedOn, modified_on AS ModifiedOn
                FROM services 
                WHERE service_id = @ServiceId AND is_deleted = false";

            var service = await connection.QueryFirstOrDefaultAsync<ServiceDto>(sql, new { ServiceId = serviceId.Value }).ConfigureAwait(false);
            
            activity.SetTag("service.found", (service != null).ToString().ToLower());
            LogQueryExecuted(_logger, nameof(GetByIdAsync), stopwatch.ElapsedMilliseconds, null);
            return service;
        }
        catch (Exception ex)
        {
            LogQueryFailed(_logger, nameof(GetByIdAsync), stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<IService?> GetBySlugAsync(IServiceSlug slug, CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: Optimized case-insensitive slug search with performance monitoring
        using var activity = new Activity("Dapper.GetServiceBySlug");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            
            const string sql = @"
                SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                       slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                       created_by AS CreatedBy, created_on AS CreatedOn, modified_on AS ModifiedOn
                FROM services 
                WHERE LOWER(slug) = LOWER(@Slug) AND is_deleted = false";

            var service = await connection.QueryFirstOrDefaultAsync<ServiceDto>(sql, new { Slug = slug.Value }).ConfigureAwait(false);
            
            activity.SetTag("service.found", (service != null).ToString().ToLower());
            activity.SetTag("slug.value", slug.Value);
            LogQueryExecuted(_logger, nameof(GetBySlugAsync), stopwatch.ElapsedMilliseconds, null);
            return service;
        }
        catch (Exception ex)
        {
            LogQueryFailed(_logger, nameof(GetBySlugAsync), stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<IEnumerable<IService>> GetByCategoryAsync(IServiceCategoryId categoryId, CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: High-performance category filtering with monitoring
        using var activity = new Activity("Dapper.GetServicesByCategory");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            
            const string sql = @"
                SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                       slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                       created_by AS CreatedBy, created_on AS CreatedOn, modified_on AS ModifiedOn
                FROM services 
                WHERE category_id = @CategoryId AND is_deleted = false
                ORDER BY created_on DESC";

            var services = await connection.QueryAsync<ServiceDto>(sql, new { CategoryId = categoryId.Value }).ConfigureAwait(false);
            
            activity.SetTag("category.id", categoryId.Value.ToString());
            activity.SetTag("services.count", services.Count().ToString());
            LogQueryExecuted(_logger, nameof(GetByCategoryAsync), stopwatch.ElapsedMilliseconds, null);
            return services;
        }
        catch (Exception ex)
        {
            LogQueryFailed(_logger, nameof(GetByCategoryAsync), stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<bool> SlugExistsAsync(IServiceSlug slug, CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: High-performance slug existence check with monitoring
        using var activity = new Activity("Dapper.CheckSlugExists");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            
            const string sql = @"
                SELECT EXISTS(
                    SELECT 1 FROM services 
                    WHERE LOWER(slug) = LOWER(@Slug) AND is_deleted = false
                )";

            var exists = await connection.QueryFirstAsync<bool>(sql, new { Slug = slug.Value }).ConfigureAwait(false);
            
            activity.SetTag("slug.value", slug.Value);
            activity.SetTag("slug.exists", exists.ToString().ToLower());
            LogQueryExecuted(_logger, nameof(SlugExistsAsync), stopwatch.ElapsedMilliseconds, null);
            return exists;
        }
        catch (Exception ex)
        {
            LogQueryFailed(_logger, nameof(SlugExistsAsync), stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: High-performance count query with monitoring
        using var activity = new Activity("Dapper.GetTotalServiceCount");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            
            const string sql = "SELECT COUNT(*) FROM services WHERE is_deleted = false";
            var count = await connection.QueryFirstAsync<int>(sql).ConfigureAwait(false);
            
            activity.SetTag("total.count", count.ToString());
            LogQueryExecuted(_logger, nameof(GetTotalCountAsync), stopwatch.ElapsedMilliseconds, null);
            return count;
        }
        catch (Exception ex)
        {
            LogQueryFailed(_logger, nameof(GetTotalCountAsync), stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<IEnumerable<IService>> SearchAsync(string searchTerm, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        // REFACTOR PHASE: High-performance search with pagination and monitoring
        using var activity = new Activity("Dapper.SearchServices");
        activity.Start();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            
            const string sql = @"
                SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                       slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                       created_by AS CreatedBy, created_on AS CreatedOn, modified_on AS ModifiedOn
                FROM services 
                WHERE (LOWER(title) LIKE LOWER(@SearchTerm) OR LOWER(description) LIKE LOWER(@SearchTerm))
                  AND is_deleted = false
                ORDER BY created_on DESC
                OFFSET @Offset LIMIT @Limit";

            var services = await connection.QueryAsync<ServiceDto>(sql, new 
            { 
                SearchTerm = $"%{searchTerm}%",
                Offset = offset,
                Limit = limit
            }).ConfigureAwait(false);
            
            activity.SetTag("search.term", searchTerm);
            activity.SetTag("search.offset", offset.ToString());
            activity.SetTag("search.limit", limit.ToString());
            activity.SetTag("results.count", services.Count().ToString());
            LogQueryExecuted(_logger, nameof(SearchAsync), stopwatch.ElapsedMilliseconds, null);
            return services;
        }
        catch (Exception ex)
        {
            LogQueryFailed(_logger, nameof(SearchAsync), stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}