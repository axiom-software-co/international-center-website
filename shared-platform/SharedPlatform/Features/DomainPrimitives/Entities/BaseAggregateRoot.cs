using SharedPlatform.Features.DomainPrimitives.DomainEvents;

namespace SharedPlatform.Features.DomainPrimitives.Entities;

public abstract class BaseAggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}