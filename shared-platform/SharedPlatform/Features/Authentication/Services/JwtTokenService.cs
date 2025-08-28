using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SharedPlatform.Features.Authentication.Abstractions;
using SharedPlatform.Features.Authentication.Models;
using SharedPlatform.Features.Authentication.Configuration;
using SharedPlatform.Features.Caching.Abstractions;

namespace SharedPlatform.Features.Authentication.Services;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly ICacheService? _cacheService;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly SigningCredentials _signingCredentials;

    private static readonly Action<ILogger, string?, Exception?> LogAccessTokenGenerated =
        LoggerMessage.Define<string?>(LogLevel.Debug, new EventId(2001, "AccessTokenGenerated"), 
            "Access token generated for user: {UserId}");

    private static readonly Action<ILogger, Exception?> LogAccessTokenGenerationFailed =
        LoggerMessage.Define(LogLevel.Error, new EventId(2002, "AccessTokenGenerationFailed"), 
            "Failed to generate access token");

    private static readonly Action<ILogger, string?, Exception?> LogRefreshTokenGenerated =
        LoggerMessage.Define<string?>(LogLevel.Debug, new EventId(2003, "RefreshTokenGenerated"), 
            "Refresh token generated for user: {UserId}");

    private static readonly Action<ILogger, string?, Exception?> LogRefreshTokenGenerationFailed =
        LoggerMessage.Define<string?>(LogLevel.Error, new EventId(2004, "RefreshTokenGenerationFailed"), 
            "Failed to generate refresh token for user: {UserId}");

    private static readonly Action<ILogger, Exception?> LogTokenExpired =
        LoggerMessage.Define(LogLevel.Warning, new EventId(2005, "TokenExpired"), 
            "Token validation failed: Token expired");

    private static readonly Action<ILogger, Exception?> LogInvalidSignature =
        LoggerMessage.Define(LogLevel.Warning, new EventId(2006, "InvalidSignature"), 
            "Token validation failed: Invalid signature");

    private static readonly Action<ILogger, Exception?> LogTokenValidationFailed =
        LoggerMessage.Define(LogLevel.Error, new EventId(2007, "TokenValidationFailed"), 
            "Token validation failed");

    private static readonly Action<ILogger, Exception?> LogTokenRevoked =
        LoggerMessage.Define(LogLevel.Information, new EventId(2008, "TokenRevoked"), 
            "Token revoked");

    private static readonly Action<ILogger, string?, Exception?> LogTokenCacheHit =
        LoggerMessage.Define<string?>(LogLevel.Debug, new EventId(2009, "TokenCacheHit"), 
            "Token validation cache hit for token: {TokenId}");

    private static readonly Action<ILogger, string?, Exception?> LogTokenCacheMiss =
        LoggerMessage.Define<string?>(LogLevel.Debug, new EventId(2010, "TokenCacheMiss"), 
            "Token validation cache miss for token: {TokenId}");

    private static readonly Action<ILogger, string?, Exception?> LogTokenBlacklisted =
        LoggerMessage.Define<string?>(LogLevel.Warning, new EventId(2011, "TokenBlacklisted"), 
            "Token is blacklisted: {TokenId}");

    public JwtTokenService(IOptions<JwtOptions> jwtOptions, ILogger<JwtTokenService> logger, ICacheService? cacheService = null)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _cacheService = cacheService;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
    }

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _jwtOptions = new JwtOptions
        {
            SecretKey = configuration["Jwt:SecretKey"] ?? "default-secret-key-for-development-only-not-production-use",
            Issuer = configuration["Jwt:Issuer"] ?? "SharedPlatform",
            Audience = configuration["Jwt:Audience"] ?? "SharedPlatform"
        };
        _logger = logger;
        _cacheService = null;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
    }

    public JwtTokenService() : this(
        new ConfigurationBuilder().Build(),
        new Mock.MockLogger<JwtTokenService>())
    {
    }

    public async Task<string> GenerateAccessTokenAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async work

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = principal.Identity as ClaimsIdentity,
                Expires = DateTime.UtcNow.Add(_jwtOptions.AccessTokenExpiration),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            LogAccessTokenGenerated(_logger, principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, null);
            return tokenString;
        }
        catch (Exception ex)
        {
            LogAccessTokenGenerationFailed(_logger, ex);
            throw;
        }
    }

    public async Task<string> GenerateRefreshTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async work

            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            
            var refreshToken = Convert.ToBase64String(randomBytes);
            
            LogRefreshTokenGenerated(_logger, userId, null);
            return refreshToken;
        }
        catch (Exception ex)
        {
            LogRefreshTokenGenerationFailed(_logger, userId, ex);
            throw;
        }
    }

    public async Task<Models.TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Models.TokenValidationResult.Failure("Token is null or empty");

        var tokenId = GetTokenId(token);

        // Check token blacklist first
        if (_jwtOptions.EnableTokenBlacklist && await IsTokenBlacklistedAsync(tokenId, cancellationToken))
        {
            LogTokenBlacklisted(_logger, tokenId, null);
            return Models.TokenValidationResult.Failure("Token has been revoked");
        }

        // Check cache if enabled
        if (_jwtOptions.EnableCaching && _cacheService != null)
        {
            var cacheKey = $"jwt_validation:{tokenId}";
            var cachedResult = await _cacheService.GetAsync<Models.TokenValidationResult>(cacheKey, cancellationToken);
            
            if (cachedResult != null)
            {
                LogTokenCacheHit(_logger, tokenId, null);
                return cachedResult;
            }
            
            LogTokenCacheMiss(_logger, tokenId, null);
        }

        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async work

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _securityKey,
                ValidateIssuer = _jwtOptions.ValidateIssuer,
                ValidIssuer = _jwtOptions.ValidateIssuer ? _jwtOptions.Issuer : null,
                ValidateAudience = _jwtOptions.ValidateAudience,
                ValidAudience = _jwtOptions.ValidateAudience ? _jwtOptions.Audience : null,
                ValidateLifetime = _jwtOptions.ValidateLifetime,
                ClockSkew = _jwtOptions.ClockSkew
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken)
            {
                var result = Models.TokenValidationResult.Success(
                    principal: principal,
                    expiresAt: jwtToken.ValidTo,
                    tokenType: "Bearer");

                // Cache successful validation if enabled
                if (_jwtOptions.EnableCaching && _cacheService != null)
                {
                    var cacheKey = $"jwt_validation:{tokenId}";
                    await _cacheService.SetAsync(cacheKey, result, _jwtOptions.CacheExpiration, cancellationToken);
                }

                return result;
            }

            return Models.TokenValidationResult.Failure("Invalid token format");
        }
        catch (SecurityTokenExpiredException)
        {
            LogTokenExpired(_logger, null);
            return Models.TokenValidationResult.Failure("Token expired");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            LogInvalidSignature(_logger, null);
            return Models.TokenValidationResult.Failure("Invalid token signature");
        }
        catch (Exception ex)
        {
            LogTokenValidationFailed(_logger, ex);
            return Models.TokenValidationResult.Failure("Token validation failed");
        }
    }

    public async Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateTokenAsync(token, cancellationToken);
        
        if (validationResult.IsValid && validationResult.Principal != null)
        {
            return validationResult.Principal;
        }

        throw new SecurityTokenValidationException(validationResult.ErrorMessage ?? "Failed to get principal from token");
    }

    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        await Task.Delay(1, cancellationToken); // Simulate async work
        
        LogTokenRevoked(_logger, null);

        if (_jwtOptions.EnableTokenBlacklist && _cacheService != null)
        {
            var tokenId = GetTokenId(token);
            var blacklistKey = $"jwt_blacklist:{tokenId}";
            
            // Add to blacklist with appropriate expiration
            var expiration = GetTokenExpiration(token) ?? _jwtOptions.AccessTokenExpiration;
            await _cacheService.SetAsync(blacklistKey, new TokenBlacklistEntry { IsBlacklisted = true }, expiration, cancellationToken);

            // Remove from validation cache if it exists
            var cacheKey = $"jwt_validation:{tokenId}";
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        }
    }

    private string GetTokenId(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value 
                   ?? jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value 
                   ?? Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)))[..16];
        }
        catch
        {
            // Fallback to hash if token parsing fails
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)))[..16];
        }
    }

    private TimeSpan? GetTokenExpiration(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            if (jwtToken.ValidTo > DateTime.UtcNow)
            {
                return jwtToken.ValidTo - DateTime.UtcNow;
            }
        }
        catch
        {
            // Token parsing failed, use default
        }
        return null;
    }

    private async Task<bool> IsTokenBlacklistedAsync(string tokenId, CancellationToken cancellationToken)
    {
        if (_cacheService == null)
            return false;

        var blacklistKey = $"jwt_blacklist:{tokenId}";
        var entry = await _cacheService.GetAsync<TokenBlacklistEntry>(blacklistKey, cancellationToken);
        return entry?.IsBlacklisted ?? false;
    }
}

internal class TokenBlacklistEntry
{
    public bool IsBlacklisted { get; set; }
    public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;
}