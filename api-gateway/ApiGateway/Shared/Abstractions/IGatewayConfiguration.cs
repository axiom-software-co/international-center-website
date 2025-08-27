namespace ApiGateway.Shared.Abstractions;

public interface IGatewayConfiguration
{
    string Environment { get; }
    bool IsPublicGateway { get; }
    bool IsAdminGateway { get; }
    string[] AllowedOrigins { get; }
    TimeSpan RequestTimeout { get; }
    T GetSection<T>(string sectionName) where T : class, new();
}