using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedPlatform.Features.ContentManagement.Abstractions;
using SharedPlatform.Features.ContentManagement.Configuration;

namespace SharedPlatform.Features.ContentManagement.Services;

/// <summary>
/// CDN URL generation service following SERVICES-SCHEMA.md specification
/// Generates consistent URLs for content delivery with Azure CDN integration
/// Pattern: https://cdn.internationalcenter.com/services/content/{service-id}/{content-hash}.html
/// </summary>
public sealed class CdnUrlGenerator : IContentUrlGenerator
{
    private readonly CdnOptions _cdnOptions;
    private readonly ILogger<CdnUrlGenerator> _logger;

    // High-performance LoggerMessage delegates
    private static readonly Action<ILogger, Guid, string, string, Exception?> LogUrlGenerated =
        LoggerMessage.Define<Guid, string, string>(LogLevel.Debug, new EventId(5001, nameof(LogUrlGenerated)),
            "CDN URL generated for service {ServiceId}: {ContentHash} -> {GeneratedUrl}");

    private static readonly Action<ILogger, string, Exception?> LogUrlValidated =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(5002, nameof(LogUrlValidated)),
            "CDN URL validation: {Url}");

    public CdnUrlGenerator(IOptions<CdnOptions> cdnOptions, ILogger<CdnUrlGenerator> logger)
    {
        _cdnOptions = cdnOptions?.Value ?? throw new ArgumentNullException(nameof(cdnOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate CDN URL following exact SERVICES-SCHEMA.md pattern
    /// Pattern: https://cdn.internationalcenter.com/services/content/{service-id}/{content-hash}.{extension}
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="contentHash">SHA256 content hash</param>
    /// <param name="extension">File extension (e.g., ".html")</param>
    /// <returns>Complete CDN URL for content access</returns>
    public async Task<string> GenerateCdnUrlAsync(Guid serviceId, string contentHash, string extension)
    {
        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId cannot be empty", nameof(serviceId));
        
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("ContentHash cannot be null or empty", nameof(contentHash));
        
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("Extension cannot be null or empty", nameof(extension));

        // Ensure extension starts with dot
        var normalizedExtension = extension.StartsWith('.') ? extension : $".{extension}";
        
        // Build URL following SERVICES-SCHEMA.md specification
        var baseUrl = _cdnOptions.BaseUrl.TrimEnd('/');
        var pathPrefix = _cdnOptions.ContentPathPrefix.TrimEnd('/');
        var url = $"{baseUrl}{pathPrefix}/{serviceId:D}/{contentHash.ToLowerInvariant()}{normalizedExtension}";

        LogUrlGenerated(_logger, serviceId, contentHash, url, null);

        return await Task.FromResult(url);
    }

    /// <summary>
    /// Generate blob storage path for internal Azure Storage operations
    /// Pattern: services/content/{service-id}/{content-hash}.{extension}
    /// </summary>
    /// <param name="serviceId">Service identifier</param>
    /// <param name="contentHash">SHA256 content hash</param>
    /// <param name="extension">File extension</param>
    /// <returns>Blob storage path</returns>
    public string GenerateBlobPath(Guid serviceId, string contentHash, string extension)
    {
        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId cannot be empty", nameof(serviceId));
        
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("ContentHash cannot be null or empty", nameof(contentHash));
        
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("Extension cannot be null or empty", nameof(extension));

        // Ensure extension starts with dot
        var normalizedExtension = extension.StartsWith('.') ? extension : $".{extension}";
        
        // Build blob path without leading slash (blob storage paths don't start with /)
        var pathPrefix = _cdnOptions.ContentPathPrefix.TrimStart('/').TrimEnd('/');
        return $"{pathPrefix}/{serviceId:D}/{contentHash.ToLowerInvariant()}{normalizedExtension}";
    }

    /// <summary>
    /// Validate content URL format against SERVICES-SCHEMA.md specification
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>True if URL matches expected pattern</returns>
    public bool IsValidContentUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            LogUrlValidated(_logger, "Invalid: null or empty URL", null);
            return false;
        }

        try
        {
            // Parse URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                LogUrlValidated(_logger, $"Invalid: malformed URL - {url}", null);
                return false;
            }

            // Validate scheme and host
            var baseUri = new Uri(_cdnOptions.BaseUrl);
            if (uri.Scheme != baseUri.Scheme || uri.Host != baseUri.Host)
            {
                LogUrlValidated(_logger, $"Invalid: scheme/host mismatch - {url}", null);
                return false;
            }

            // Validate path structure
            var expectedPathPrefix = _cdnOptions.ContentPathPrefix.TrimEnd('/');
            if (!uri.AbsolutePath.StartsWith(expectedPathPrefix))
            {
                LogUrlValidated(_logger, $"Invalid: path prefix mismatch - {url}", null);
                return false;
            }

            // Extract and validate path segments
            var pathAfterPrefix = uri.AbsolutePath[expectedPathPrefix.Length..].TrimStart('/');
            var segments = pathAfterPrefix.Split('/');

            // Should have 2 segments: {service-id}/{content-hash}.{extension}
            if (segments.Length != 2)
            {
                LogUrlValidated(_logger, $"Invalid: incorrect path segments - {url}", null);
                return false;
            }

            // Validate service ID format (GUID)
            if (!Guid.TryParse(segments[0], out _))
            {
                LogUrlValidated(_logger, $"Invalid: service ID format - {url}", null);
                return false;
            }

            // Validate content hash and extension format
            var filenamePart = segments[1];
            var dotIndex = filenamePart.LastIndexOf('.');
            if (dotIndex <= 0 || dotIndex >= filenamePart.Length - 1)
            {
                LogUrlValidated(_logger, $"Invalid: filename format - {url}", null);
                return false;
            }

            var contentHash = filenamePart[..dotIndex];
            var extension = filenamePart[dotIndex..];

            // Validate content hash format (64 character hex string for SHA256)
            if (contentHash.Length != 64 || !IsHexString(contentHash))
            {
                LogUrlValidated(_logger, $"Invalid: content hash format - {url}", null);
                return false;
            }

            // Validate extension
            if (string.IsNullOrWhiteSpace(extension) || !extension.StartsWith('.'))
            {
                LogUrlValidated(_logger, $"Invalid: extension format - {url}", null);
                return false;
            }

            LogUrlValidated(_logger, $"Valid: {url}", null);
            return true;
        }
        catch (Exception)
        {
            LogUrlValidated(_logger, $"Invalid: exception during validation - {url}", null);
            return false;
        }
    }

    /// <summary>
    /// Check if string is valid hexadecimal
    /// </summary>
    private static bool IsHexString(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && 
               value.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
    }
}