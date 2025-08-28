using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

public sealed class PublishingStatus : BaseValueObject
{
    public static readonly string[] ValidValues = { "draft", "published", "archived" };
    
    public string Value { get; private set; }

    private PublishingStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PublishingStatus cannot be null or empty", nameof(value));
        
        var normalizedValue = value.Trim().ToLowerInvariant();
        if (!ValidValues.Contains(normalizedValue))
            throw new ArgumentException($"Invalid PublishingStatus: {value}. Valid values are: {string.Join(", ", ValidValues)}", nameof(value));
        
        Value = normalizedValue;
    }

    public static PublishingStatus Draft => new("draft");
    public static PublishingStatus Published => new("published");
    public static PublishingStatus Archived => new("archived");
    
    public static PublishingStatus From(string value) => new(value);

    public bool IsDraft => Value == "draft";
    public bool IsPublished => Value == "published";
    public bool IsArchived => Value == "archived";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(PublishingStatus status) => status.Value;
    public static implicit operator PublishingStatus(string value) => From(value);
    
    public override string ToString() => Value;
}