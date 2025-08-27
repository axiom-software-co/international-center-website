using SharedPlatform.Features.Authentication.Models;
using System.Security.Claims;

namespace SharedPlatform.Features.Authentication.Abstractions;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<string> GenerateRefreshTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
}