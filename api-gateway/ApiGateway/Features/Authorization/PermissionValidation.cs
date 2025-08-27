using System.Security.Claims;

namespace ApiGateway.Features.Authorization;

public static class PermissionValidation
{
    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        throw new NotImplementedException();
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