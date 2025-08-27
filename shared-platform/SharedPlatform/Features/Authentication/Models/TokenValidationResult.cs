using System.Security.Claims;

namespace SharedPlatform.Features.Authentication.Models;

public class TokenValidationResult
{
    public bool IsValid { get; init; }
    public ClaimsPrincipal? Principal { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public string? TokenType { get; init; }
    
    public static TokenValidationResult Success(ClaimsPrincipal principal, DateTimeOffset? expiresAt = null, string? tokenType = null)
    {
        return new TokenValidationResult
        {
            IsValid = true,
            Principal = principal,
            ExpiresAt = expiresAt,
            TokenType = tokenType
        };
    }
    
    public static TokenValidationResult Failure(string errorMessage)
    {
        return new TokenValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}