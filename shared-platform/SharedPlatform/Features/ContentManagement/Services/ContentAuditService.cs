using SharedPlatform.Features.ContentManagement.Abstractions;
using SharedPlatform.Features.DataAccess.Abstractions;
using SharedPlatform.Features.ResultHandling;
using Microsoft.Extensions.Logging;

namespace SharedPlatform.Features.ContentManagement.Services;

public sealed class ContentAuditService : IContentAuditService
{
    private readonly IAuditDataAccess _auditDataAccess;
    private readonly ILogger<ContentAuditService> _logger;

    // High-performance LoggerMessage delegates
    private static readonly Action<ILogger, string, Guid, string, Exception?> LogContentAudit =
        LoggerMessage.Define<string, Guid, string>(LogLevel.Information, new EventId(2001, nameof(LogContentAudit)),
            "Content audit logged: {Operation} for service {ServiceId} with URL {ContentUrl}");

    private static readonly Action<ILogger, Guid, Exception?> LogAuditFailed =
        LoggerMessage.Define<Guid>(LogLevel.Error, new EventId(2002, nameof(LogAuditFailed)),
            "Failed to log content audit for service {ServiceId}");

    private static readonly Action<ILogger, Guid, string, string, Exception?> LogContextCreated =
        LoggerMessage.Define<Guid, string, string>(LogLevel.Debug, new EventId(2003, nameof(LogContextCreated)),
            "Audit context created for service {ServiceId}, operation {Operation}, user {UserId}");

    private static readonly Action<ILogger, Guid, Exception?> LogContextFailed =
        LoggerMessage.Define<Guid>(LogLevel.Error, new EventId(2004, nameof(LogContextFailed)),
            "Failed to create audit context for service {ServiceId}");

    public ContentAuditService(
        IAuditDataAccess auditDataAccess,
        ILogger<ContentAuditService> logger)
    {
        _auditDataAccess = auditDataAccess;
        _logger = logger;
    }

    public async Task<Result<ContentAuditEntry>> LogContentOperationAsync(
        Guid serviceId,
        string contentUrl,
        string operation,
        string userId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditEntry = new ContentAuditEntry
            {
                AuditId = Guid.NewGuid(),
                ServiceId = serviceId,
                ContentUrl = contentUrl,
                OperationType = operation,
                UserId = userId,
                CorrelationId = correlationId,
                AuditTimestamp = DateTime.UtcNow,
                AuditContext = new { Operation = operation, ContentUrl = contentUrl }
            };

            await _auditDataAccess.CreateContentAuditAsync(auditEntry);

            LogContentAudit(_logger, operation, serviceId, contentUrl, null);

            return Result<ContentAuditEntry>.Success(auditEntry);
        }
        catch (Exception ex)
        {
            LogAuditFailed(_logger, serviceId, ex);
            return Result<ContentAuditEntry>.Failure(
                Error.Failure("ContentAudit.Log.Failed", 
                    $"Failed to log content audit: {ex.Message}"));
        }
    }

    public async Task<ContentAuditContext> CreateAuditContextAsync(
        Guid serviceId,
        string contentHash,
        string operation,
        string userId)
    {
        try
        {
            var auditContext = new ContentAuditContext
            {
                ContentHash = contentHash,
                ContentSize = 0, // Will be set by caller
                ContentType = "text/html", // Default
                Metadata = new Dictionary<string, object>
                {
                    ["ServiceId"] = serviceId,
                    ["Operation"] = operation,
                    ["UserId"] = userId,
                    ["Timestamp"] = DateTime.UtcNow
                },
                ComplianceTags = ["content-audit", "service-operation"]
            };

            LogContextCreated(_logger, serviceId, operation, userId, null);

            return auditContext;
        }
        catch (Exception ex)
        {
            LogContextFailed(_logger, serviceId, ex);
            throw;
        }
    }

    // Convenience method with different signature for backward compatibility
    public async Task<Result> LogContentOperationAsync(
        string operation,
        string serviceId,
        string contentHash,
        object? additionalData = null)
    {
        try
        {
            var guid = Guid.TryParse(serviceId, out var parsedGuid) ? parsedGuid : Guid.NewGuid();
            var result = await LogContentOperationAsync(
                guid,
                $"hash:{contentHash}",
                operation,
                "system",
                Guid.NewGuid(),
                CancellationToken.None);
            
            return result.IsSuccess ? Result.Success() : Result.Failure(result.Error!);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("ContentAudit.Log.Failed", ex.Message));
        }
    }

    // Convenience method for backward compatibility
    public async Task<Result<ContentAuditContext>> CreateAuditContextAsync(
        string operation,
        string serviceId,
        object? contextData = null)
    {
        try
        {
            var guid = Guid.TryParse(serviceId, out var parsedGuid) ? parsedGuid : Guid.NewGuid();
            var context = await CreateAuditContextAsync(guid, "unknown", operation, "system");
            return Result<ContentAuditContext>.Success(context);
        }
        catch (Exception ex)
        {
            return Result<ContentAuditContext>.Failure(Error.Failure("ContentAudit.Context.Failed", ex.Message));
        }
    }
}