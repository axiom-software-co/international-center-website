using SharedPlatform.Features.DomainPrimitives.DomainEvents;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.DomainPrimitives.Entities;

public abstract class BaseAggregateRoot : BaseEntity
{
    private readonly ConcurrentQueue<IDomainEvent> _domainEvents = new();
    private volatile bool _hasEvents;
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents
    {
        get
        {
            if (!_hasEvents)
                return Array.Empty<IDomainEvent>();
                
            return _domainEvents.ToArray();
        }
    }
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        
        _domainEvents.Enqueue(domainEvent);
        _hasEvents = true;
    }
    
    public void ClearDomainEvents()
    {
        while (_domainEvents.TryDequeue(out _)) { }
        _hasEvents = false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasDomainEvents() => _hasEvents;
}

public abstract class BaseAggregateRoot<TId> : BaseEntity<TId> where TId : notnull
{
    private readonly ConcurrentQueue<IDomainEvent> _domainEvents = new();
    private volatile bool _hasEvents;
    
    protected BaseAggregateRoot(TId id) : base(id) { }
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents
    {
        get
        {
            if (!_hasEvents)
                return Array.Empty<IDomainEvent>();
                
            return _domainEvents.ToArray();
        }
    }
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        
        _domainEvents.Enqueue(domainEvent);
        _hasEvents = true;
    }
    
    public void ClearDomainEvents()
    {
        while (_domainEvents.TryDequeue(out _)) { }
        _hasEvents = false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasDomainEvents() => _hasEvents;
}