using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiGateway.Features.Authorization;

public class AdminAuthorizationStrategy : IAuthorizationStrategy
{
    public string Name => "Admin";
    
    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, HttpContext context, string policy, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Minimal implementation for medical-grade admin authorization
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return Task.FromResult(AuthorizationResult.Failed());
        }
        
        // Check for admin role
        if (user.IsInRole("Admin") || user.IsInRole("Administrator"))
        {
            return Task.FromResult(AuthorizationResult.Success());
        }
        
        return Task.FromResult(AuthorizationResult.Failed());
    }
    
    public bool CanHandle(string policy)
    {
        return policy.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
               policy.Equals("AdminOnly", StringComparison.OrdinalIgnoreCase);
    }
}