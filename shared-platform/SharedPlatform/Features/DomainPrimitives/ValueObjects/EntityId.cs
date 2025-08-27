namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public class EntityId : BaseValueObject
{
    public Guid Value { get; private set; }
    
    private EntityId(Guid value)
    {
        Value = value;
    }
    
    public static EntityId New() => new(Guid.NewGuid());
    
    public static EntityId From(Guid value) => new(value);
    
    public static EntityId From(string value) => new(Guid.Parse(value));
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(EntityId entityId) => entityId.Value;
    
    public static implicit operator EntityId(Guid value) => new(value);
}