using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.DomainPrimitives.Entities;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    private int? _cachedHashCode;
    
    public abstract object Id { get; }
    
    public bool Equals(BaseEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        
        return Id.Equals(other.Id);
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseEntity);
    }
    
    public override int GetHashCode()
    {
        if (_cachedHashCode.HasValue)
            return _cachedHashCode.Value;
            
        _cachedHashCode = ComputeHashCode();
        return _cachedHashCode.Value;
    }
    
    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ComputeHashCode()
    {
        const int seed = 17;
        const int multiplier = 23;
        
        return unchecked(seed * multiplier + Id.GetHashCode());
    }
}

public abstract class BaseEntity<TId> : BaseEntity where TId : notnull
{
    private readonly TId _id;
    
    protected BaseEntity(TId id)
    {
        _id = id;
    }
    
    public TId TypedId => _id;
    
    public override object Id => _id;
}