namespace ApiGateway.Features.Observability;

public class ObservabilityConfiguration
{
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableMetricsCollection { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public bool IncludeRequestBody { get; set; } = true;
    public bool IncludeResponseBody { get; set; } = true;
    public TimeSpan MetricsFlushInterval { get; set; } = TimeSpan.FromSeconds(30);
    public HashSet<string> SensitiveFields { get; set; } = new() { "UserId", "Password", "Email" };
    public HashSet<string> SensitiveHeaders { get; set; } = new() { "Authorization", "X-API-Key", "Cookie" };

    public Dictionary<string, object> RedactSensitiveData(Dictionary<string, object> data)
    {
        var redacted = new Dictionary<string, object>();
        
        foreach (var kvp in data)
        {
            redacted[kvp.Key] = SensitiveFields.Contains(kvp.Key) || SensitiveHeaders.Contains(kvp.Key) 
                ? "[REDACTED]" 
                : kvp.Value;
        }
        
        return redacted;
    }

    public bool IsValid()
    {
        return MetricsFlushInterval > TimeSpan.Zero;
    }
}
