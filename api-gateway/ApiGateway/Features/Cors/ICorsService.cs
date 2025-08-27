using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.Cors;

public interface ICorsService
{
    Task ApplyCorsAsync(HttpContext context);
    bool IsOriginAllowed(string origin);
}