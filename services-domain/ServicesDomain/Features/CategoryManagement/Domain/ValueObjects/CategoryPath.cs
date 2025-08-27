using SharedPlatform.Features.DomainPrimitives.ValueObjects;

namespace ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;

public sealed class CategoryPath : BaseValueObject
{
    public const int MaxLength = 500;
    public const string Separator = "/";
    
    public string Value { get; private set; }
    public string[] Segments { get; private set; }
    public int Depth { get; private set; }

    private CategoryPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CategoryPath cannot be null or empty", nameof(value));
        
        if (value.Length > MaxLength)
            throw new ArgumentException($"CategoryPath cannot exceed {MaxLength} characters", nameof(value));
        
        Value = value.Trim();
        Segments = Value.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
        Depth = Segments.Length;
    }

    public static CategoryPath From(string value) => new(value);
    
    public static CategoryPath FromSegments(params string[] segments)
    {
        if (segments == null || segments.Length == 0)
            throw new ArgumentException("Segments cannot be null or empty", nameof(segments));
        
        var path = string.Join(Separator, segments.Where(s => !string.IsNullOrWhiteSpace(s)));
        return new CategoryPath(path);
    }

    public CategoryPath GetParent()
    {
        if (Depth <= 1)
            throw new InvalidOperationException("Cannot get parent of root category");
        
        var parentSegments = Segments.Take(Depth - 1);
        return FromSegments(parentSegments.ToArray());
    }

    public CategoryPath AddChild(string childName)
    {
        if (string.IsNullOrWhiteSpace(childName))
            throw new ArgumentException("Child name cannot be null or empty", nameof(childName));
        
        var newSegments = Segments.Concat(new[] { childName }).ToArray();
        return FromSegments(newSegments);
    }

    public bool IsParentOf(CategoryPath other)
    {
        if (other == null || other.Depth <= Depth)
            return false;
        
        return other.Value.StartsWith(Value + Separator, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsChildOf(CategoryPath other)
    {
        if (other == null || Depth <= other.Depth)
            return false;
        
        return Value.StartsWith(other.Value + Separator, StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(CategoryPath path) => path.Value;
    public static implicit operator CategoryPath(string value) => From(value);
    
    public override string ToString() => Value;
}