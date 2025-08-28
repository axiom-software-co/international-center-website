using SharedPlatform.Features.Configuration.Options;
using SharedPlatform.Features.ResultHandling;
using System.ComponentModel.DataAnnotations;

namespace SharedPlatform.Features.ContentManagement.Configuration;

/// <summary>
/// CDN configuration for content delivery following SERVICES-SCHEMA.md specification
/// Configures Azure CDN integration with proper URL patterns
/// </summary>
public sealed class CdnOptions : BaseOptions
{
    /// <summary>
    /// Configuration section name for options pattern
    /// </summary>
    public const string SectionName = "Cdn";

    /// <summary>
    /// CDN base URL following SERVICES-SCHEMA.md pattern
    /// Required format: https://cdn.internationalcenter.com
    /// </summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://cdn.internationalcenter.com";

    /// <summary>
    /// Content path prefix for URL generation
    /// Default: "/services/content"
    /// </summary>
    [Required]
    public string ContentPathPrefix { get; set; } = "/services/content";

    /// <summary>
    /// Cache control header value for CDN optimization
    /// Default: "public, max-age=31536000" (1 year for immutable content)
    /// </summary>
    public string CacheControlHeader { get; set; } = "public, max-age=31536000, immutable";

    /// <summary>
    /// Enable URL signing for security (if supported)
    /// Default: false
    /// </summary>
    public bool EnableUrlSigning { get; set; } = false;

    /// <summary>
    /// URL signing key for secure URLs
    /// Required if EnableUrlSigning is true
    /// </summary>
    public string? SigningKey { get; set; }

    /// <summary>
    /// CDN purge endpoint for cache invalidation
    /// </summary>
    public string? PurgeEndpoint { get; set; }

    /// <summary>
    /// CDN authentication key for purge operations
    /// </summary>
    public string? PurgeAuthKey { get; set; }

    /// <summary>
    /// Enable automatic cache purging on content updates
    /// Default: true
    /// </summary>
    public bool EnableAutoPurge { get; set; } = true;

    /// <summary>
    /// Default cache TTL in seconds for content
    /// Default: 31536000 (1 year for immutable content)
    /// </summary>
    [Range(300, 31536000)] // 5 minutes to 1 year
    public int DefaultCacheTtlSeconds { get; set; } = 31536000;

    /// <summary>
    /// Validates CDN configuration
    /// </summary>
    public override Result Validate()
    {
        var baseResult = base.Validate();
        if (!baseResult.IsSuccess)
            return baseResult;

        // Validate base URL format
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return Result.Failure(Error.Validation("Cdn.BaseUrl.Required", "BaseUrl is required"));
        
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            return Result.Failure(Error.Validation("Cdn.BaseUrl.Invalid", "BaseUrl must be a valid HTTPS URL"));

        // Validate content path prefix
        if (string.IsNullOrWhiteSpace(ContentPathPrefix))
            return Result.Failure(Error.Validation("Cdn.ContentPathPrefix.Required", "ContentPathPrefix is required"));
        
        if (!ContentPathPrefix.StartsWith('/'))
            return Result.Failure(Error.Validation("Cdn.ContentPathPrefix.Invalid", "ContentPathPrefix must start with '/'"));

        // Validate URL signing configuration
        if (EnableUrlSigning && string.IsNullOrWhiteSpace(SigningKey))
            return Result.Failure(Error.Validation("Cdn.SigningKey.Required", "SigningKey is required when EnableUrlSigning is true"));

        // Validate purge configuration
        if (EnableAutoPurge)
        {
            if (string.IsNullOrWhiteSpace(PurgeEndpoint))
                return Result.Failure(Error.Validation("Cdn.PurgeEndpoint.Required", "PurgeEndpoint is required when EnableAutoPurge is true"));

            if (string.IsNullOrWhiteSpace(PurgeAuthKey))
                return Result.Failure(Error.Validation("Cdn.PurgeAuthKey.Required", "PurgeAuthKey is required when EnableAutoPurge is true"));
        }

        return Result.Success();
    }

    /// <summary>
    /// Generate the complete URL pattern for content
    /// Returns: {BaseUrl}{ContentPathPrefix}/{serviceId}/{contentHash}.{extension}
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:URI-like return values should not be strings", 
        Justification = "Returns a template string with placeholders, not a valid URI")]
    public string GetUrlPattern()
    {
        var baseUrl = BaseUrl.TrimEnd('/');
        var pathPrefix = ContentPathPrefix.TrimEnd('/');
        return $"{baseUrl}{pathPrefix}/{{serviceId}}/{{contentHash}}.{{extension}}";
    }

    /// <summary>
    /// Validate if a URL matches the expected CDN pattern
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>True if URL matches CDN pattern</returns>
    public bool IsValidCdnUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        var baseUri = new Uri(BaseUrl);
        if (uri.Host != baseUri.Host || uri.Scheme != baseUri.Scheme)
            return false;

        return uri.AbsolutePath.StartsWith(ContentPathPrefix);
    }
}