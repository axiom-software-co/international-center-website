using SharedPlatform.Features.DomainPrimitives.DomainEvents;
using Xunit;

namespace SharedPlatform.Features.DomainPrimitives.Entities;

public class DomainPrimitivesTests
{
    private class TestEntity : BaseEntity
    {
        public TestEntity(Guid id) => Id = id;
        public override object Id { get; }
    }
    
    private class TestAggregateRoot : BaseAggregateRoot
    {
        public TestAggregateRoot(Guid id) => Id = id;
        public override object Id { get; }
        
        public void TriggerEvent(IDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }
    }
    
    private class TestDomainEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    }
    
    private class AuditableEntity : BaseEntity, IAuditable
    {
        public AuditableEntity(Guid id) => Id = id;
        public override object Id { get; }
        public DateTimeOffset CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
    
    private class SoftDeletableEntity : BaseEntity, ISoftDeletable
    {
        public SoftDeletableEntity(Guid id) => Id = id;
        public override object Id { get; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
    }
    
    private class VersionedEntity : BaseEntity, IVersioned
    {
        public VersionedEntity(Guid id) => Id = id;
        public override object Id { get; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
    
    [Fact]
    public void BaseEntity_ShouldHaveId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        
        Assert.Equal(id, entity.Id);
    }
    
    [Fact]
    public void BaseAggregateRoot_ShouldManageDomainEvents()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();
        
        Assert.Empty(aggregate.DomainEvents);
        
        aggregate.TriggerEvent(domainEvent);
        
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
        
        aggregate.ClearDomainEvents();
        
        Assert.Empty(aggregate.DomainEvents);
    }
    
    [Fact]
    public void ISoftDeletable_ShouldTrackDeletion()
    {
        var entity = new SoftDeletableEntity(Guid.NewGuid());
        var deletedOn = DateTimeOffset.UtcNow;
        var deletedBy = "test-user";
        
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedOn);
        Assert.Null(entity.DeletedBy);
        
        entity.IsDeleted = true;
        entity.DeletedOn = deletedOn;
        entity.DeletedBy = deletedBy;
        
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedOn, entity.DeletedOn);
        Assert.Equal(deletedBy, entity.DeletedBy);
    }
    
    [Fact]
    public void IAuditable_ShouldTrackAuditInfo()
    {
        var entity = new AuditableEntity(Guid.NewGuid());
        var createdOn = DateTimeOffset.UtcNow;
        var createdBy = "test-user";
        var modifiedOn = DateTimeOffset.UtcNow.AddMinutes(5);
        var modifiedBy = "test-modifier";
        
        entity.CreatedOn = createdOn;
        entity.CreatedBy = createdBy;
        entity.ModifiedOn = modifiedOn;
        entity.ModifiedBy = modifiedBy;
        
        Assert.Equal(createdOn, entity.CreatedOn);
        Assert.Equal(createdBy, entity.CreatedBy);
        Assert.Equal(modifiedOn, entity.ModifiedOn);
        Assert.Equal(modifiedBy, entity.ModifiedBy);
    }
    
    [Fact]
    public void IVersioned_ShouldHaveRowVersion()
    {
        var entity = new VersionedEntity(Guid.NewGuid());
        var rowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        
        Assert.NotNull(entity.RowVersion);
        
        entity.RowVersion = rowVersion;
        
        Assert.Equal(rowVersion, entity.RowVersion);
    }
}