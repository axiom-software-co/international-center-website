namespace SharedPlatform.Features.DomainPrimitives.DomainEvents;

public abstract class BaseDomainEvent : IDomainEvent
{
    protected BaseDomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
    }
    
    public Guid Id { get; }
    public DateTimeOffset OccurredOn { get; }
}