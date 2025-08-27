using SharedPlatform.Features.Authentication.Models;
using System.Security.Claims;

namespace SharedPlatform.Features.Authentication.Abstractions;

public interface IUserContext
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    ClaimsPrincipal? Principal { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool HasClaim(string type, string value);
    UserPrincipal? GetUserPrincipal();
}