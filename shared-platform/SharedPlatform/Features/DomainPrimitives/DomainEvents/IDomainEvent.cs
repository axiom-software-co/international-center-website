namespace SharedPlatform.Features.DomainPrimitives.DomainEvents;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredOn { get; }
}