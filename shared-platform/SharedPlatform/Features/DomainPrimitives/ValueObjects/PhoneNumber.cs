using System.Text.RegularExpressions;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public class PhoneNumber : BaseValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+[1-9]\d{1,14}$",
        RegexOptions.Compiled);
    
    public string Value { get; private set; }
    
    private PhoneNumber(string value)
    {
        Value = value;
    }
    
    public static PhoneNumber From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(value));
            
        var cleanValue = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        if (!PhoneRegex.IsMatch(cleanValue))
            throw new ArgumentException($"'{value}' is not a valid phone number.", nameof(value));
            
        return new PhoneNumber(cleanValue);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}