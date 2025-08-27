using Microsoft.AspNetCore.Http;

namespace ApiGateway.Shared.Abstractions;

public interface IGatewayService
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    Task ProcessRequestAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task<string> GetGatewayVersionAsync();
}