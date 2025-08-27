namespace SharedPlatform.Features.DomainPrimitives.Entities;

public interface IVersioned
{
    byte[] RowVersion { get; }
}