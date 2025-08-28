using SharedPlatform.Features.Configuration.Options;
using SharedPlatform.Features.ResultHandling;
using System.ComponentModel.DataAnnotations;

namespace SharedPlatform.Features.ContentManagement.Configuration;

/// <summary>
/// Content lifecycle and cleanup configuration
/// Manages retention policies and cleanup schedules for compliance
/// </summary>
public sealed class ContentLifecycleOptions : BaseOptions
{
    /// <summary>
    /// Configuration section name for options pattern
    /// </summary>
    public const string SectionName = "ContentLifecycle";

    /// <summary>
    /// Default audit retention period in days
    /// Default: 2555 days (7 years for regulatory compliance)
    /// </summary>
    [Range(30, 3650)] // 30 days to 10 years
    public int AuditRetentionDays { get; set; } = 2555;

    /// <summary>
    /// Orphaned content grace period before cleanup in days
    /// Default: 30 days
    /// </summary>
    [Range(1, 365)]
    public int OrphanedContentGraceDays { get; set; } = 30;

    /// <summary>
    /// Enable automatic cleanup of orphaned content
    /// Default: true
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;

    /// <summary>
    /// Cleanup schedule in cron format
    /// Default: "0 2 * * 0" (Sunday at 2 AM)
    /// </summary>
    public string CleanupSchedule { get; set; } = "0 2 * * 0";

    /// <summary>
    /// Maximum number of items to process per cleanup batch
    /// Default: 1000
    /// </summary>
    [Range(10, 10000)]
    public int CleanupBatchSize { get; set; } = 1000;

    /// <summary>
    /// Enable content archiving before deletion
    /// Default: true for compliance
    /// </summary>
    public bool EnableArchiving { get; set; } = true;

    /// <summary>
    /// Archive storage configuration
    /// </summary>
    public ArchiveStorageOptions ArchiveStorage { get; set; } = new();

    /// <summary>
    /// Enable cleanup audit logging
    /// Default: true
    /// </summary>
    public bool EnableCleanupAuditing { get; set; } = true;

    /// <summary>
    /// Cleanup operation timeout in minutes
    /// Default: 60 minutes
    /// </summary>
    [Range(5, 480)] // 5 minutes to 8 hours
    public int CleanupTimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Validates lifecycle configuration
    /// </summary>
    public override Result Validate()
    {
        var baseResult = base.Validate();
        if (!baseResult.IsSuccess)
            return baseResult;

        // Validate retention period is longer than grace period
        if (AuditRetentionDays <= OrphanedContentGraceDays)
        {
            return Result.Failure(Error.Validation("ContentLifecycle.RetentionPeriod.Invalid", 
                "AuditRetentionDays must be greater than OrphanedContentGraceDays"));
        }

        // Validate cron schedule format (basic validation)
        if (string.IsNullOrWhiteSpace(CleanupSchedule))
            return Result.Failure(Error.Validation("ContentLifecycle.CleanupSchedule.Required", "CleanupSchedule is required"));
        
        var cronParts = CleanupSchedule.Split(' ');
        if (cronParts.Length != 5)
        {
            return Result.Failure(Error.Validation("ContentLifecycle.CleanupSchedule.Invalid", 
                "CleanupSchedule must be a valid 5-part cron expression"));
        }

        // Validate archiving configuration
        if (EnableArchiving && ArchiveStorage != null)
        {
            var archiveValidation = ArchiveStorage.Validate();
            if (!archiveValidation.IsSuccess)
            {
                return Result.Failure(Error.Validation("ContentLifecycle.ArchiveStorage.Invalid", 
                    $"ArchiveStorage validation failed: {archiveValidation.Error?.Message}"));
            }
        }

        return Result.Success();
    }
}

/// <summary>
/// Archive storage configuration for content lifecycle
/// </summary>
public sealed class ArchiveStorageOptions : BaseOptions
{
    /// <summary>
    /// Archive storage connection string (separate from active storage)
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Archive container name
    /// Default: "services-content-archive"
    /// </summary>
    [Required]
    public string ContainerName { get; set; } = "services-content-archive";

    /// <summary>
    /// Archive storage access tier for cost optimization
    /// Default: "Archive" (lowest cost)
    /// </summary>
    public string AccessTier { get; set; } = "Archive";

    /// <summary>
    /// Enable archive compression for storage efficiency
    /// Default: true
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Archive retention period in days
    /// Default: 3650 days (10 years)
    /// </summary>
    [Range(365, 7300)] // 1 year to 20 years
    public int RetentionDays { get; set; } = 3650;

    /// <summary>
    /// Validates archive storage configuration
    /// </summary>
    public override Result Validate()
    {
        var baseResult = base.Validate();
        if (!baseResult.IsSuccess)
            return baseResult;

        if (string.IsNullOrWhiteSpace(ConnectionString))
            return Result.Failure(Error.Validation("ArchiveStorage.ConnectionString.Required", "ConnectionString is required"));

        if (string.IsNullOrWhiteSpace(ContainerName))
            return Result.Failure(Error.Validation("ArchiveStorage.ContainerName.Required", "ContainerName is required"));

        var validAccessTiers = new[] { "Hot", "Cool", "Cold", "Archive" };
        if (!validAccessTiers.Contains(AccessTier, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(Error.Validation("ArchiveStorage.AccessTier.Invalid", 
                $"AccessTier must be one of: {string.Join(", ", validAccessTiers)}"));
        }

        return Result.Success();
    }
}