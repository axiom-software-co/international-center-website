using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public sealed class EntityId : BaseValueObject
{
    public static readonly EntityId Empty = new(Guid.Empty);
    
    public Guid Value { get; }
    
    private EntityId(Guid value)
    {
        Value = value;
    }
    
    public static EntityId New() => new(Guid.NewGuid());
    
    public static EntityId From(Guid value) => value == Guid.Empty ? Empty : new(value);
    
    public static EntityId From(string value)
    {
        return TryFrom(value, out var entityId) 
            ? entityId 
            : throw new ArgumentException($"'{value}' is not a valid GUID.", nameof(value));
    }
    
    public static bool TryFrom(string? value, [NotNullWhen(true)] out EntityId? entityId)
    {
        entityId = null;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;
            
        if (!Guid.TryParse(value, out var guid))
            return false;
            
        entityId = From(guid);
        return true;
    }
    
    public static bool TryFrom(Guid value, [NotNullWhen(true)] out EntityId? entityId)
    {
        entityId = From(value);
        return true;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value.ToString();
    
    public string ToString(string format) => Value.ToString(format);
    
    public string ToShortString() => Value.ToString("N")[..8];
    
    public static implicit operator Guid(EntityId entityId) => entityId.Value;
    
    public static implicit operator EntityId(Guid value) => From(value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty() => Value == Guid.Empty;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNotEmpty() => Value != Guid.Empty;
    
    public byte[] ToByteArray() => Value.ToByteArray();
    
    public static EntityId FromByteArray(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        
        if (bytes.Length != 16)
            throw new ArgumentException("Byte array must be exactly 16 bytes.", nameof(bytes));
            
        return From(new Guid(bytes));
    }
}