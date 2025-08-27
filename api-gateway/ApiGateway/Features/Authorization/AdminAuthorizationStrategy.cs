using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiGateway.Features.Authorization;

public class AdminAuthorizationStrategy : IAuthorizationStrategy
{
    public string Name => "Admin";
    
    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, HttpContext context, string policy, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public bool CanHandle(string policy)
    {
        return policy.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
               policy.Equals("AdminOnly", StringComparison.OrdinalIgnoreCase);
    }
}