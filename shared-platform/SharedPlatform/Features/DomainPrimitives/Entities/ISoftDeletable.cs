namespace SharedPlatform.Features.DomainPrimitives.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedOn { get; }
    string? DeletedBy { get; }
}