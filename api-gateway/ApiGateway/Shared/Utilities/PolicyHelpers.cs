using Microsoft.AspNetCore.Authorization;
using ApiGateway.Shared.Models;

namespace ApiGateway.Shared.Utilities;

public static class PolicyHelpers
{
    public static AuthorizationPolicy CreatePolicyFromDefinition(PolicyDefinition definition)
    {
        throw new NotImplementedException();
    }
    
    public static bool IsAuthorized(this AuthorizationResult result)
    {
        return result.Succeeded;
    }
    
    public static string[] GetRequiredRoles(PolicyDefinition definition)
    {
        throw new NotImplementedException();
    }
    
    public static string[] GetRequiredPermissions(PolicyDefinition definition)
    {
        throw new NotImplementedException();
    }
    
    public static PolicyDefinition CreateRoleBasedPolicy(string name, params string[] roles)
    {
        throw new NotImplementedException();
    }
}