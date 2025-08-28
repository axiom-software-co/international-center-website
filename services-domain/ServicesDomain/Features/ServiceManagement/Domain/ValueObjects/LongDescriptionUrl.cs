using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class LongDescriptionUrl : BaseValueObject
{
    public const int MaxLength = 500;
    
    public string? Value { get; private set; }

    private LongDescriptionUrl(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var trimmedValue = value.Trim();
            
            if (trimmedValue.Length > MaxLength)
                throw new ArgumentException($"LongDescriptionUrl cannot exceed {MaxLength} characters", nameof(value));
            
            if (!Uri.TryCreate(trimmedValue, UriKind.Absolute, out var uri) || 
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                throw new ArgumentException("LongDescriptionUrl must be a valid HTTP or HTTPS URL", nameof(value));
            
            Value = trimmedValue;
        }
        else
        {
            Value = null;
        }
    }

    public static LongDescriptionUrl From(string? value) => new(value);
    public static LongDescriptionUrl Empty => new(null);

    public bool HasValue => !string.IsNullOrWhiteSpace(Value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value ?? string.Empty;
    }

    public static implicit operator string?(LongDescriptionUrl url) => url.Value;
    public static implicit operator LongDescriptionUrl(string? value) => From(value);
    
    public override string ToString() => Value ?? string.Empty;
}