using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGateway.Features.Authentication;

public interface IAuthenticationStrategy
{
    string Name { get; }
    Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task ChallengeAsync(HttpContext context, CancellationToken cancellationToken = default);
    bool CanHandle(HttpContext context);
}