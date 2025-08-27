namespace SharedPlatform.Features.DomainPrimitives.Entities;

public interface IAuditable
{
    DateTimeOffset CreatedOn { get; }
    string? CreatedBy { get; }
    DateTimeOffset? ModifiedOn { get; }
    string? ModifiedBy { get; }
}