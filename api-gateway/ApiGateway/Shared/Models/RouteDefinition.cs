namespace ApiGateway.Shared.Models;

public class RouteDefinition
{
    public string RouteId { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public string Match { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string[] Methods { get; set; } = Array.Empty<string>();
    public int Priority { get; set; }
    public bool RequiresAuthentication { get; set; }
    public string[] RequiredRoles { get; set; } = Array.Empty<string>();
}