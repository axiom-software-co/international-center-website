using System.Security.Claims;

namespace SharedPlatform.Features.Authentication.Models;

public class AuthenticationResult
{
    public bool IsSuccessful { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public ClaimsPrincipal? Principal { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    
    public static AuthenticationResult Success(string accessToken, string? refreshToken = null, ClaimsPrincipal? principal = null, DateTimeOffset? expiresAt = null)
    {
        return new AuthenticationResult
        {
            IsSuccessful = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Principal = principal,
            ExpiresAt = expiresAt
        };
    }
    
    public static AuthenticationResult Failure(string errorMessage)
    {
        return new AuthenticationResult
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage
        };
    }
}