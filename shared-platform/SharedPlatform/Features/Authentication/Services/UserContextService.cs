using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedPlatform.Features.Authentication.Abstractions;
using SharedPlatform.Features.Authentication.Models;

namespace SharedPlatform.Features.Authentication.Services;

public class UserContextService : IUserContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ILogger<UserContextService> _logger;
    private ClaimsPrincipal? _currentPrincipal;
    
    private static readonly Action<ILogger, string?, Exception?> LogUserContextAccessed =
        LoggerMessage.Define<string?>(LogLevel.Debug, new EventId(3001, "UserContextAccessed"), 
            "User context accessed for user: {UserId}");

    private static readonly Action<ILogger, string?, string?, Exception?> LogRoleCheckAccessed =
        LoggerMessage.Define<string?, string?>(LogLevel.Debug, new EventId(3002, "RoleCheckAccessed"), 
            "Role check performed for user: {UserId}, role: {Role}");

    private static readonly Action<ILogger, string?, string?, string?, Exception?> LogClaimCheckAccessed =
        LoggerMessage.Define<string?, string?, string?>(LogLevel.Debug, new EventId(3003, "ClaimCheckAccessed"), 
            "Claim check performed for user: {UserId}, type: {ClaimType}, value: {ClaimValue}");

    private static readonly Action<ILogger, string?, Exception?> LogUserPrincipalCreated =
        LoggerMessage.Define<string?>(LogLevel.Debug, new EventId(3004, "UserPrincipalCreated"), 
            "User principal created for user: {UserId}");

    public UserContextService(
        IHttpContextAccessor? httpContextAccessor = null,
        ILogger<UserContextService>? logger = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger ?? new Mock.NullLogger<UserContextService>();
    }

    public string? UserId 
    { 
        get 
        {
            var userId = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
                LogUserContextAccessed(_logger, userId, null);
            return userId;
        }
    }

    public string? UserName => Principal?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value;

    public IReadOnlyList<string> Roles => 
        Principal?.FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList() ?? 
        new List<string>();

    public ClaimsPrincipal? Principal => 
        _currentPrincipal ?? 
        _httpContextAccessor?.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        var result = Principal?.IsInRole(role) ?? false;
        LogRoleCheckAccessed(_logger, UserId, role, null);
        return result;
    }

    public bool HasClaim(string type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var result = Principal?.HasClaim(type, value) ?? false;
        LogClaimCheckAccessed(_logger, UserId, type, value, null);
        return result;
    }

    public UserPrincipal? GetUserPrincipal()
    {
        if (Principal == null)
            return null;

        var userPrincipal = UserPrincipal.FromClaimsPrincipal(Principal);
        LogUserPrincipalCreated(_logger, UserId, null);
        return userPrincipal;
    }

    public void SetCurrentUser(ClaimsPrincipal principal)
    {
        _currentPrincipal = principal;
    }
}

internal static partial class Mock
{
    public class NullLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}