using System.Security.Claims;

namespace ApiGateway.Features.Authorization;

public static class PermissionValidation
{
    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        // GREEN PHASE: Minimal implementation for medical-grade permission validation
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }
        
        // Check for permission claims
        return user.HasClaim("permission", permission) || 
               user.HasClaim("permissions", permission) ||
               user.HasClaim(ClaimTypes.AuthorizationDecision, permission);
    }
    
    public static bool HasRole(ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }
    
    public static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(role => user.IsInRole(role));
    }
    
    public static bool HasAllRoles(ClaimsPrincipal user, params string[] roles)
    {
        return roles.All(role => user.IsInRole(role));
    }
    
    public static string[] GetUserRoles(ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }
}