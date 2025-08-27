using SharedPlatform.Features.DomainPrimitives.ValueObjects;
using System.Text.RegularExpressions;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class ServiceSlug : BaseValueObject
{
    public const int MaxLength = 100;
    public const int MinLength = 3;
    private static readonly Regex SlugPattern = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    
    public string Value { get; private set; }

    private ServiceSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ServiceSlug cannot be null or empty", nameof(value));
        
        if (value.Length < MinLength)
            throw new ArgumentException($"ServiceSlug must be at least {MinLength} characters", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"ServiceSlug cannot exceed {MaxLength} characters", nameof(value));
        
        if (!SlugPattern.IsMatch(value))
            throw new ArgumentException("ServiceSlug must contain only lowercase letters, numbers, and hyphens", nameof(value));
        
        Value = value;
    }

    public static ServiceSlug From(string value) => new(value);
    
    public static ServiceSlug FromTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("'", "")
            .Replace("\"", "");
        
        // Remove multiple hyphens and trim
        slug = Regex.Replace(slug, "-+", "-").Trim('-');
        
        if (slug.Length > MaxLength)
            slug = slug[..MaxLength].TrimEnd('-');
        
        return new ServiceSlug(slug);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(ServiceSlug slug) => slug.Value;
    public static implicit operator ServiceSlug(string value) => From(value);
    
    public override string ToString() => Value;
}