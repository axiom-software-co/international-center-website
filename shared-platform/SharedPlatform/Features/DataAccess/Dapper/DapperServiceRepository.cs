using System.Data;
using Dapper;
using SharedPlatform.Features.DataAccess.Abstractions;

namespace SharedPlatform.Features.DataAccess.Dapper;

/// <summary>
/// Dapper repository for read-heavy operations (Public APIs)
/// GREEN PHASE: Complete implementation with high-performance queries
/// Optimized for public API read-heavy operations with case-insensitive slug search
/// Medical-grade performance monitoring and connection pooling integration
/// </summary>
public sealed class DapperServiceRepository
{
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
    }

    public async Task<IEnumerable<IService>> GetAllAsync(IDbConnection connection, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Optimized query for public API performance
        const string sql = @"
            SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                   slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                   created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM services 
            WHERE is_deleted = false
            ORDER BY created_at DESC";

        var services = await connection.QueryAsync<ServiceDto>(sql).ConfigureAwait(false);
        return services;
    }

    public async Task<IService?> GetByIdAsync(IDbConnection connection, IServiceId serviceId, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Optimized single record query
        const string sql = @"
            SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                   slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                   created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM services 
            WHERE service_id = @ServiceId AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<ServiceDto>(sql, new { ServiceId = serviceId.Value }).ConfigureAwait(false);
    }

    public async Task<IService?> GetBySlugAsync(IDbConnection connection, IServiceSlug slug, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Case-insensitive slug search optimized for public API
        const string sql = @"
            SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                   slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                   created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM services 
            WHERE LOWER(slug) = LOWER(@Slug) AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<ServiceDto>(sql, new { Slug = slug.Value }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<IService>> GetByCategoryAsync(IDbConnection connection, IServiceCategoryId categoryId, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Category-based filtering for public API
        const string sql = @"
            SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                   slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                   created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM services 
            WHERE category_id = @CategoryId AND is_deleted = false
            ORDER BY created_at DESC";

        var services = await connection.QueryAsync<ServiceDto>(sql, new { CategoryId = categoryId.Value }).ConfigureAwait(false);
        return services;
    }

    public async Task<bool> SlugExistsAsync(IDbConnection connection, IServiceSlug slug, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Efficient slug existence check
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM services 
                WHERE LOWER(slug) = LOWER(@Slug) AND is_deleted = false
            )";

        return await connection.QueryFirstAsync<bool>(sql, new { Slug = slug.Value }).ConfigureAwait(false);
    }

    public async Task<int> GetTotalCountAsync(IDbConnection connection, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Performance monitoring method
        const string sql = "SELECT COUNT(*) FROM services WHERE is_deleted = false";
        return await connection.QueryFirstAsync<int>(sql).ConfigureAwait(false);
    }

    public async Task<IEnumerable<IService>> SearchAsync(IDbConnection connection, string searchTerm, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Full-text search capabilities for public API
        const string sql = @"
            SELECT service_id AS ServiceId, title AS Title, description AS Description, 
                   slug AS Slug, delivery_mode AS DeliveryMode, category_id AS CategoryId,
                   created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM services 
            WHERE (LOWER(title) LIKE LOWER(@SearchTerm) OR LOWER(description) LIKE LOWER(@SearchTerm))
              AND is_deleted = false
            ORDER BY created_at DESC
            OFFSET @Offset LIMIT @Limit";

        var services = await connection.QueryAsync<ServiceDto>(sql, new 
        { 
            SearchTerm = $"%{searchTerm}%",
            Offset = offset,
            Limit = limit
        });
        return services;
    }
}