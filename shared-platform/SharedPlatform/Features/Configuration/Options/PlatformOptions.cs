using SharedPlatform.Features.ResultHandling;
using System.Text.RegularExpressions;

namespace SharedPlatform.Features.Configuration.Options;

public partial class PlatformOptions : BaseOptions
{
    public string Environment { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public override Result Validate()
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(Environment))
            validationErrors.Add("Environment is required");

        if (string.IsNullOrWhiteSpace(ApplicationName))
            validationErrors.Add("ApplicationName is required");

        if (string.IsNullOrWhiteSpace(Version))
            validationErrors.Add("Version is required");
        else if (!IsValidVersion(Version))
            validationErrors.Add("Version must be in semantic version format (e.g., 1.0.0)");

        return validationErrors.Count == 0
            ? Result.Success()
            : Error.Validation("PlatformOptions.Validation", string.Join("; ", validationErrors));
    }

    private static bool IsValidVersion(string version)
    {
        return VersionRegex().IsMatch(version);
    }

    [GeneratedRegex(@"^\d+\.\d+\.\d+(-\w+)?(\.\d+)?$", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();
}