using SharedPlatform.Features.ContentManagement.Abstractions;
using SharedPlatform.Features.ContentManagement.Configuration;
using SharedPlatform.Features.DataAccess.Abstractions;
using SharedPlatform.Features.ResultHandling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SharedPlatform.Features.ContentManagement.Services;

public sealed class ContentStorageService : IContentStorageService
{
    private readonly IContentHashService _contentHashService;
    private readonly IContentUrlGenerator _urlGenerator;
    private readonly IContentAuditService _auditService;
    private readonly IServiceDataAccess _serviceDataAccess;
    private readonly ContentStorageOptions _options;
    private readonly ILogger<ContentStorageService> _logger;

    // High-performance LoggerMessage delegates
    private static readonly Action<ILogger, string, Exception?> LogUploadSimulation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(3001, nameof(LogUploadSimulation)),
            "Simulating upload to blob storage: {BlobPath}");

    private static readonly Action<ILogger, Guid, string, Exception?> LogContentUploaded =
        LoggerMessage.Define<Guid, string>(LogLevel.Information, new EventId(3002, nameof(LogContentUploaded)),
            "Content uploaded successfully for service {ServiceId} with hash {ContentHash}");

    private static readonly Action<ILogger, Guid, Exception?> LogUploadFailed =
        LoggerMessage.Define<Guid>(LogLevel.Error, new EventId(3003, nameof(LogUploadFailed)),
            "Failed to upload content for service {ServiceId}");

    private static readonly Action<ILogger, Guid, string, Exception?> LogServiceUpdated =
        LoggerMessage.Define<Guid, string>(LogLevel.Information, new EventId(3004, nameof(LogServiceUpdated)),
            "Service {ServiceId} updated with content URL {ContentUrl}");

    private static readonly Action<ILogger, Guid, Exception?> LogUpdateFailed =
        LoggerMessage.Define<Guid>(LogLevel.Error, new EventId(3005, nameof(LogUpdateFailed)),
            "Failed to upload and update service {ServiceId}");

    public ContentStorageService(
        IContentHashService contentHashService,
        IContentUrlGenerator urlGenerator,
        IContentAuditService auditService,
        IServiceDataAccess serviceDataAccess,
        IOptions<ContentStorageOptions> options,
        ILogger<ContentStorageService> logger)
    {
        _contentHashService = contentHashService;
        _urlGenerator = urlGenerator;
        _auditService = auditService;
        _serviceDataAccess = serviceDataAccess;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<ContentUploadResult>> UploadContentAsync(
        Guid serviceId,
        string content,
        string contentType,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Generate content hash for immutable versioning
            var contentHash = _contentHashService.GenerateContentHash(content);
            
            // Generate blob path and CDN URL
            var blobPath = _urlGenerator.GenerateBlobPath(serviceId, contentHash, GetExtensionFromContentType(contentType));
            var cdnUrl = await _urlGenerator.GenerateCdnUrlAsync(serviceId, contentHash, GetExtensionFromContentType(contentType));
            
            // Simulate blob storage upload (placeholder implementation)
            LogUploadSimulation(_logger, blobPath, null);
            
            // Simulate upload delay
            await Task.Delay(10, cancellationToken);

            // Log content operation for audit compliance
            await _auditService.LogContentOperationAsync(
                serviceId,
                cdnUrl,
                "ContentUpload",
                userId,
                Guid.NewGuid(),
                cancellationToken);

            var result = new ContentUploadResult
            {
                ContentUrl = cdnUrl,
                ContentHash = contentHash,
                BlobPath = blobPath,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = userId,
                ContentSize = System.Text.Encoding.UTF8.GetByteCount(content)
            };

            LogContentUploaded(_logger, serviceId, contentHash, null);

            return Result<ContentUploadResult>.Success(result);
        }
        catch (Exception ex)
        {
            LogUploadFailed(_logger, serviceId, ex);
            return Result<ContentUploadResult>.Failure(
                Error.Failure("ContentStorage.Upload.Failed", 
                    $"Failed to upload content: {ex.Message}"));
        }
    }

    public async Task<Result<ContentUploadResult>> UploadAndUpdateServiceAsync(
        IServiceId serviceId,
        string content,
        string contentType,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Upload content first
            var uploadResult = await UploadContentAsync(serviceId.Value, content, contentType, userId, cancellationToken);
            if (!uploadResult.IsSuccess)
                return uploadResult;

            // Update service record with CDN URL
            var service = await _serviceDataAccess.GetByIdAsync(serviceId.Value);
            if (service == null)
            {
                return Result<ContentUploadResult>.Failure(
                    Error.NotFound("ContentStorage.Service.NotFound", 
                        $"Service not found: {serviceId.Value}"));
            }

            // Update service with content URL and hash
            service.ContentUrl = uploadResult.Value.ContentUrl;
            service.ContentHash = uploadResult.Value.ContentHash;
            service.LastContentUpdate = uploadResult.Value.UploadedAt;

            await _serviceDataAccess.UpdateAsync(service);

            // Log service update for audit compliance
            await _auditService.LogContentOperationAsync(
                serviceId.Value,
                uploadResult.Value.ContentUrl,
                "ServiceContentUpdate",
                userId,
                Guid.NewGuid(),
                cancellationToken);

            LogServiceUpdated(_logger, serviceId.Value, uploadResult.Value.ContentUrl, null);

            return uploadResult;
        }
        catch (Exception ex)
        {
            LogUpdateFailed(_logger, serviceId.Value, ex);
            return Result<ContentUploadResult>.Failure(
                Error.Failure("ContentStorage.UploadAndUpdate.Failed", 
                    $"Failed to upload and update service: {ex.Message}"));
        }
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "text/html" => "html",
            "application/json" => "json",
            "text/plain" => "txt",
            _ => "html"
        };
    }
}