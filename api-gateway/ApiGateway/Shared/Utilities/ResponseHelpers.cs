using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ApiGateway.Shared.Utilities;

public static class ResponseHelpers
{
    public static async Task WriteJsonAsync<T>(this HttpResponse response, T data, JsonSerializerOptions? options = null)
    {
        throw new NotImplementedException();
    }
    
    public static async Task WriteErrorAsync(this HttpResponse response, int statusCode, string message, string? detail = null)
    {
        throw new NotImplementedException();
    }
    
    public static void SetSecurityHeaders(this HttpResponse response)
    {
        throw new NotImplementedException();
    }
    
    public static void SetCacheHeaders(this HttpResponse response, TimeSpan maxAge)
    {
        throw new NotImplementedException();
    }
    
    public static void SetCorrelationId(this HttpResponse response, string correlationId)
    {
        response.Headers["X-Correlation-ID"] = correlationId;
    }
}