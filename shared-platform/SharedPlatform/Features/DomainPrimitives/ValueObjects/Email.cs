using System.Text.RegularExpressions;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public class Email : BaseValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public string Value { get; private set; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Email From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty.", nameof(value));
            
        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));
            
        return new Email(value.ToLowerInvariant());
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(Email email) => email.Value;
}