using System.Text.RegularExpressions;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public class Slug : BaseValueObject
{
    private static readonly Regex SlugRegex = new(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled);
    
    public string Value { get; private set; }
    
    private Slug(string value)
    {
        Value = value;
    }
    
    public static Slug From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug cannot be null or empty.", nameof(value));
            
        var slug = GenerateSlug(value);
        
        if (!SlugRegex.IsMatch(slug))
            throw new ArgumentException($"'{value}' cannot be converted to a valid slug.", nameof(value));
            
        return new Slug(slug);
    }
    
    public static Slug FromString(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be null or empty.", nameof(slug));
            
        if (!SlugRegex.IsMatch(slug))
            throw new ArgumentException($"'{slug}' is not a valid slug.", nameof(slug));
            
        return new Slug(slug);
    }
    
    private static string GenerateSlug(string input)
    {
        return input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Trim('-');
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(Slug slug) => slug.Value;
}