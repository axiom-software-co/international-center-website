using Microsoft.AspNetCore.Builder;
using ApiGateway.Shared.Abstractions;

namespace ApiGateway.Shared.Services;

public class MiddlewarePipeline : IMiddlewareProvider
{
    private readonly Dictionary<Type, int> _middlewarePriorities = new();
    
    public IApplicationBuilder ConfigureMiddleware(IApplicationBuilder app)
    {
        throw new NotImplementedException();
    }
    
    public IEnumerable<Type> GetMiddlewareTypes()
    {
        throw new NotImplementedException();
    }
    
    public int GetPriority(Type middlewareType)
    {
        return _middlewarePriorities.GetValueOrDefault(middlewareType, 0);
    }
    
    public void SetMiddlewarePriority(Type middlewareType, int priority)
    {
        _middlewarePriorities[middlewareType] = priority;
    }
}