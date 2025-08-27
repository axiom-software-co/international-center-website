using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiGateway.Features.Authorization;

public class PublicAuthorizationStrategy : IAuthorizationStrategy
{
    public string Name => "Public";
    
    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, HttpContext context, string policy, CancellationToken cancellationToken = default)
    {
        // Public endpoints allow all requests
        return Task.FromResult(AuthorizationResult.Success());
    }
    
    public bool CanHandle(string policy)
    {
        return policy.Equals("Public", StringComparison.OrdinalIgnoreCase) ||
               policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase);
    }
}