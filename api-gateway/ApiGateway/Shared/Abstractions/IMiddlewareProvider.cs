using Microsoft.AspNetCore.Builder;

namespace ApiGateway.Shared.Abstractions;

public interface IMiddlewareProvider
{
    IApplicationBuilder ConfigureMiddleware(IApplicationBuilder app);
    IEnumerable<Type> GetMiddlewareTypes();
    int GetPriority(Type middlewareType);
}