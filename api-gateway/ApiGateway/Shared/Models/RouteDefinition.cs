namespace ApiGateway.Shared.Models;

public class RouteDefinition
{
    public string RouteId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public string? AuthorizationPolicy { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
