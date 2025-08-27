using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiGateway.Features.Authorization;

public interface IAuthorizationStrategy
{
    string Name { get; }
    Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, HttpContext context, string policy, CancellationToken cancellationToken = default);
    bool CanHandle(string policy);
}