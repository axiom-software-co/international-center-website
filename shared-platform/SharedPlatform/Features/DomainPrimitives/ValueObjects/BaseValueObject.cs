namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public abstract class BaseValueObject : IEquatable<BaseValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public bool Equals(BaseValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseValueObject);
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) => current * 23 + (obj?.GetHashCode() ?? 0));
    }
    
    public static bool operator ==(BaseValueObject? left, BaseValueObject? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    public static bool operator !=(BaseValueObject? left, BaseValueObject? right)
    {
        return !(left == right);
    }
}