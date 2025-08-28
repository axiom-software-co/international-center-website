namespace SharedPlatform.Features.Configuration.Options;

public class FeatureFlag
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
}