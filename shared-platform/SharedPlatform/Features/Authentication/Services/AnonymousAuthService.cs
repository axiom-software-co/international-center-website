using System.Security.Claims;
using Microsoft.Extensions.Logging;
using SharedPlatform.Features.Authentication.Abstractions;
using SharedPlatform.Features.Authentication.Models;

namespace SharedPlatform.Features.Authentication.Services;

public class AnonymousAuthService : IAuthenticationService
{
    private readonly ILogger<AnonymousAuthService>? _logger;

    private static readonly Action<ILogger, Exception?> LogAnonymousAuthenticationRequested =
        LoggerMessage.Define(LogLevel.Debug, new EventId(3001, "AnonymousAuthenticationRequested"), 
            "Anonymous authentication requested");

    public AnonymousAuthService(ILogger<AnonymousAuthService>? logger = null)
    {
        _logger = logger;
    }

    public Task<AuthenticationResult> AuthenticateAsync(string token, CancellationToken cancellationToken = default)
    {
        if (_logger != null)
            LogAnonymousAuthenticationRequested(_logger, null);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "anonymous"),
            new Claim(ClaimTypes.Name, "Anonymous User"),
            new Claim(ClaimTypes.Role, "Anonymous")
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(AuthenticationResult.Success(
            accessToken: "anonymous",
            principal: principal));
    }

    public Task<AuthenticationResult> AuthenticateAsync(AuthenticationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_logger != null)
            LogAnonymousAuthenticationRequested(_logger, null);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "anonymous"),
            new Claim(ClaimTypes.Name, "Anonymous User"),
            new Claim(ClaimTypes.Role, "Anonymous")
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(AuthenticationResult.Success(
            accessToken: "anonymous",
            principal: principal));
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(token == "anonymous");
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}