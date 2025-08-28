using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SharedPlatform.Features.Authentication.Abstractions;
using SharedPlatform.Features.Authentication.Models;
using SharedPlatform.Features.Authentication.Configuration;

namespace SharedPlatform.Features.Authentication.Services;

public class JwtAuthenticationService : IAuthenticationService
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<JwtAuthenticationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly JwtOptions _jwtOptions;

    private static readonly Action<ILogger, string?, Exception?> LogTokenValidationFailed =
        LoggerMessage.Define<string?>(LogLevel.Warning, new EventId(1001, "TokenValidationFailed"), 
            "Token validation failed: {Error}");

    private static readonly Action<ILogger, Exception?> LogAuthenticationFailed =
        LoggerMessage.Define(LogLevel.Error, new EventId(1002, "AuthenticationFailed"), 
            "Authentication failed for token");

    private static readonly Action<ILogger, Exception?> LogAuthenticationContextFailed =
        LoggerMessage.Define(LogLevel.Error, new EventId(1003, "AuthenticationContextFailed"), 
            "Authentication failed for context");

    private static readonly Action<ILogger, Exception?> LogTokenValidationError =
        LoggerMessage.Define(LogLevel.Error, new EventId(1004, "TokenValidationError"), 
            "Token validation failed");

    private static readonly Action<ILogger, Exception?> LogUserSignedOut =
        LoggerMessage.Define(LogLevel.Information, new EventId(1005, "UserSignedOut"), 
            "User signed out");

    private static readonly Action<ILogger, string?, Exception?> LogInvalidCredentials =
        LoggerMessage.Define<string?>(LogLevel.Warning, new EventId(1006, "InvalidCredentials"), 
            "Invalid credentials provided for user: {Username}");

    private static readonly Action<ILogger, Exception?> LogInvalidAuthenticationContext =
        LoggerMessage.Define(LogLevel.Warning, new EventId(1007, "InvalidAuthenticationContext"), 
            "Invalid authentication context provided");

    public JwtAuthenticationService(
        ITokenService tokenService,
        ILogger<JwtAuthenticationService> logger,
        IConfiguration configuration,
        IOptions<JwtOptions> jwtOptions)
    {
        _tokenService = tokenService;
        _logger = logger;
        _configuration = configuration;
        _jwtOptions = jwtOptions.Value;
    }

    public JwtAuthenticationService(
        ITokenService tokenService,
        ILogger<JwtAuthenticationService> logger,
        IConfiguration configuration)
    {
        _tokenService = tokenService;
        _logger = logger;
        _configuration = configuration;
        _jwtOptions = new JwtOptions();
    }

    public JwtAuthenticationService() : this(
        new JwtTokenService(), 
        new Mock.MockLogger<JwtAuthenticationService>(),
        new ConfigurationBuilder().Build())
    {
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        try
        {
            var validationResult = await _tokenService.ValidateTokenAsync(token, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                LogTokenValidationFailed(_logger, validationResult.ErrorMessage, null);
                return AuthenticationResult.Failure(validationResult.ErrorMessage ?? "Token validation failed");
            }

            return AuthenticationResult.Success(
                accessToken: token,
                principal: validationResult.Principal,
                expiresAt: validationResult.ExpiresAt);
        }
        catch (Exception ex)
        {
            LogAuthenticationFailed(_logger, ex);
            return AuthenticationResult.Failure("Authentication failed");
        }
    }

    public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            if (!string.IsNullOrWhiteSpace(context.Token))
            {
                return await AuthenticateAsync(context.Token, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(context.Username) && !string.IsNullOrWhiteSpace(context.Password))
            {
                return await AuthenticateWithCredentialsAsync(context.Username, context.Password, cancellationToken);
            }

            LogInvalidAuthenticationContext(_logger, null);
            return AuthenticationResult.Failure("Invalid authentication context");
        }
        catch (Exception ex)
        {
            LogAuthenticationContextFailed(_logger, ex);
            return AuthenticationResult.Failure("Authentication failed");
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var result = await _tokenService.ValidateTokenAsync(token, cancellationToken);
            return result.IsValid;
        }
        catch (Exception ex)
        {
            LogTokenValidationError(_logger, ex);
            return false;
        }
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        LogUserSignedOut(_logger, null);
        return Task.CompletedTask;
    }

    private async Task<AuthenticationResult> AuthenticateWithCredentialsAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken)
    {
        if (!IsValidCredentials(username, password))
        {
            LogInvalidCredentials(_logger, username, null);
            return AuthenticationResult.Failure("Invalid credentials");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, $"{username}@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        var accessToken = await _tokenService.GenerateAccessTokenAsync(principal, cancellationToken);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(username, cancellationToken);

        return AuthenticationResult.Success(
            accessToken: accessToken,
            refreshToken: refreshToken,
            principal: principal,
            expiresAt: DateTimeOffset.UtcNow.Add(_jwtOptions.AccessTokenExpiration));
    }

    private static bool IsValidCredentials(string username, string password)
    {
        return !string.IsNullOrWhiteSpace(username) && 
               !string.IsNullOrWhiteSpace(password) && 
               password.Length >= 6;
    }
}

internal static partial class Mock
{
    public class MockLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}