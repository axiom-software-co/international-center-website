using SharedPlatform.Features.Configuration.Options;
using SharedPlatform.Features.ResultHandling;
using System.ComponentModel.DataAnnotations;

namespace SharedPlatform.Features.ContentManagement.Configuration;

/// <summary>
/// Azure Blob Storage configuration for content management
/// Follows SharedPlatform configuration patterns with validation
/// </summary>
public sealed class ContentStorageOptions : BaseOptions
{
    /// <summary>
    /// Configuration section name for options pattern
    /// </summary>
    public const string SectionName = "ContentStorage";

    /// <summary>
    /// Azure Storage connection string
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Container name for content storage
    /// Default: "services-content"
    /// </summary>
    [Required]
    public string ContainerName { get; set; } = "services-content";

    /// <summary>
    /// Enable content compression for storage optimization
    /// Default: true
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Content upload timeout in seconds
    /// Default: 300 (5 minutes)
    /// </summary>
    [Range(30, 1800)]
    public int UploadTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum content size in bytes
    /// Default: 50MB
    /// </summary>
    [Range(1024, 100_000_000)] // 1KB to 100MB
    public long MaxContentSizeBytes { get; set; } = 50_000_000;

    /// <summary>
    /// Enable access tier optimization for cost management
    /// Default: true
    /// </summary>
    public bool EnableAccessTierOptimization { get; set; } = true;

    /// <summary>
    /// Default access tier for new content
    /// Options: Hot, Cool, Cold, Archive
    /// Default: "Hot"
    /// </summary>
    public string DefaultAccessTier { get; set; } = "Hot";

    /// <summary>
    /// Enable server-side encryption
    /// Default: true
    /// </summary>
    public bool EnableEncryption { get; set; } = true;

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public RetryPolicyOptions RetryPolicy { get; set; } = new();

    /// <summary>
    /// Validates configuration values
    /// </summary>
    public override Result Validate()
    {
        var baseResult = base.Validate();
        if (!baseResult.IsSuccess)
            return baseResult;

        if (string.IsNullOrWhiteSpace(ConnectionString))
            return Result.Failure(Error.Validation("ContentStorage.ConnectionString.Required", "ConnectionString is required"));

        if (string.IsNullOrWhiteSpace(ContainerName))
            return Result.Failure(Error.Validation("ContentStorage.ContainerName.Required", "ContainerName is required"));

        var validAccessTiers = new[] { "Hot", "Cool", "Cold", "Archive" };
        if (!validAccessTiers.Contains(DefaultAccessTier, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(Error.Validation("ContentStorage.DefaultAccessTier.Invalid", 
                $"DefaultAccessTier must be one of: {string.Join(", ", validAccessTiers)}"));
        }

        return Result.Success();
    }
}

/// <summary>
/// Retry policy configuration for Azure Storage operations
/// </summary>
public sealed class RetryPolicyOptions
{
    /// <summary>
    /// Maximum retry attempts
    /// Default: 3
    /// </summary>
    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay between retries in milliseconds
    /// Default: 1000 (1 second)
    /// </summary>
    [Range(100, 30000)]
    public int BaseDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Maximum delay between retries in milliseconds
    /// Default: 10000 (10 seconds)
    /// </summary>
    [Range(1000, 60000)]
    public int MaxDelayMilliseconds { get; set; } = 10000;

    /// <summary>
    /// Enable exponential backoff
    /// Default: true
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
}