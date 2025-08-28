namespace ApiGateway.Features.Cors;

public interface ICorsPolicy
{
    string Name { get; }
    CorsConfiguration GetConfiguration();
    bool IsOriginAllowed(string origin);
}