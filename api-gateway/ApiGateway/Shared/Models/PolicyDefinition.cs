namespace ApiGateway.Shared.Models;

public class PolicyDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; }
    public string[] ApplicableRoutes { get; set; } = Array.Empty<string>();
}