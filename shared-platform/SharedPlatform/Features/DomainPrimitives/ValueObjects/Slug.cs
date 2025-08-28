using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public sealed class Slug : BaseValueObject
{
    private static readonly Regex SlugRegex = new(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(50));
        
    private static readonly Regex InvalidCharsRegex = new(
        @"[^a-z0-9\-\s]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
    private const int MinSlugLength = 1;
    private const int MaxSlugLength = 100;
    
    public string Value { get; }
    
    private Slug(string value)
    {
        Value = value;
    }
    
    public static Slug From(string value)
    {
        return TryFrom(value, out var slug) 
            ? slug 
            : throw new ArgumentException(GetValidationErrorMessage(value), nameof(value));
    }
    
    public static Slug FromString(string slug)
    {
        return TryFromString(slug, out var result) 
            ? result 
            : throw new ArgumentException($"'{slug}' is not a valid slug.", nameof(slug));
    }
    
    public static bool TryFrom(string? value, [NotNullWhen(true)] out Slug? slug)
    {
        slug = null;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;
            
        var generatedSlug = GenerateSlug(value);
        
        if (string.IsNullOrEmpty(generatedSlug) || 
            generatedSlug.Length < MinSlugLength || 
            generatedSlug.Length > MaxSlugLength)
            return false;
            
        if (!IsValidSlugFormat(generatedSlug))
            return false;
            
        slug = new Slug(generatedSlug);
        return true;
    }
    
    public static bool TryFromString(string? value, [NotNullWhen(true)] out Slug? slug)
    {
        slug = null;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;
            
        if (value.Length < MinSlugLength || value.Length > MaxSlugLength)
            return false;
            
        if (!IsValidSlugFormat(value))
            return false;
            
        slug = new Slug(value);
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidSlugFormat(string value)
    {
        try
        {
            return SlugRegex.IsMatch(value);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
    
    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
            
        var normalized = input.ToLowerInvariant().Trim();
        
        // Remove diacritics (accents)
        normalized = RemoveDiacritics(normalized);
        
        // Replace spaces and underscores with hyphens first
        normalized = normalized.Replace(' ', '-').Replace('_', '-');
        
        // Remove invalid characters (excluding hyphens which are already processed)
        normalized = InvalidCharsRegex.Replace(normalized, string.Empty);
        
        // Remove consecutive hyphens
        while (normalized.Contains("--"))
        {
            normalized = normalized.Replace("--", "-");
        }
        
        // Trim hyphens from start and end
        return normalized.Trim('-');
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    
    private static string GetValidationErrorMessage(string? value)
    {
        return value switch
        {
            null => "Slug cannot be null.",
            "" => "Slug cannot be empty.",
            _ when value.Length > MaxSlugLength => $"Slug cannot exceed {MaxSlugLength} characters.",
            _ => $"'{value}' cannot be converted to a valid slug."
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(Slug slug) => slug.Value;
    
    public int WordCount => Value.Split('-', StringSplitOptions.RemoveEmptyEntries).Length;
    
    public string[] Words => Value.Split('-', StringSplitOptions.RemoveEmptyEntries);
}