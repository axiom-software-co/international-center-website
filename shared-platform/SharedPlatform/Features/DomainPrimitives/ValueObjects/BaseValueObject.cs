using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public abstract class BaseValueObject : IEquatable<BaseValueObject>
{
    private int? _cachedHashCode;
    
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public bool Equals(BaseValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        
        return EqualityComponentsEqual(GetEqualityComponents(), other.GetEqualityComponents());
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseValueObject);
    }
    
    public override int GetHashCode()
    {
        if (_cachedHashCode.HasValue)
            return _cachedHashCode.Value;
            
        _cachedHashCode = ComputeHashCode();
        return _cachedHashCode.Value;
    }
    
    public static bool operator ==(BaseValueObject? left, BaseValueObject? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    public static bool operator !=(BaseValueObject? left, BaseValueObject? right)
    {
        return !(left == right);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualityComponentsEqual(IEnumerable<object> left, IEnumerable<object> right)
    {
        using var leftEnumerator = left.GetEnumerator();
        using var rightEnumerator = right.GetEnumerator();
        
        while (leftEnumerator.MoveNext())
        {
            if (!rightEnumerator.MoveNext())
                return false;
                
            if (!Equals(leftEnumerator.Current, rightEnumerator.Current))
                return false;
        }
        
        return !rightEnumerator.MoveNext();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ComputeHashCode()
    {
        const int seed = 487;
        const int multiplier = 31;
        
        var hash = seed;
        foreach (var component in GetEqualityComponents())
        {
            hash = unchecked(hash * multiplier + (component?.GetHashCode() ?? 0));
        }
        
        return hash;
    }
}