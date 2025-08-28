using Xunit;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public class ValueObjectTests
{
    private class TestValueObject : BaseValueObject
    {
        public string Value { get; }
        
        public TestValueObject(string value)
        {
            Value = value;
        }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
    
    [Fact]
    public void BaseValueObject_ShouldImplementEquality()
    {
        var valueObject1 = new TestValueObject("test");
        var valueObject2 = new TestValueObject("test");
        var valueObject3 = new TestValueObject("different");
        
        Assert.Equal(valueObject1, valueObject2);
        Assert.NotEqual(valueObject1, valueObject3);
        Assert.True(valueObject1 == valueObject2);
        Assert.False(valueObject1 == valueObject3);
        Assert.Equal(valueObject1.GetHashCode(), valueObject2.GetHashCode());
    }
    
    [Fact]
    public void EntityId_ShouldGenerateUniqueIds()
    {
        var id1 = EntityId.New();
        var id2 = EntityId.New();
        var guid = Guid.NewGuid();
        var id3 = EntityId.From(guid);
        var id4 = EntityId.From(guid.ToString());
        
        Assert.NotEqual(id1, id2);
        Assert.Equal(id3, id4);
        Assert.Equal(guid, id3.Value);
        
        Guid implicitGuid = id1;
        EntityId implicitEntityId = guid;
        Assert.Equal(id1.Value, implicitGuid);
        Assert.Equal(guid, implicitEntityId.Value);
    }
    
    [Fact]
    public void Email_ShouldValidateEmailFormat()
    {
        var validEmail = "test@example.com";
        var email = Email.From(validEmail);
        
        Assert.Equal(validEmail.ToLowerInvariant(), email.Value);
        
        string implicitString = email;
        Assert.Equal(validEmail.ToLowerInvariant(), implicitString);
        
        Assert.Throws<ArgumentException>(() => Email.From(""));
        Assert.Throws<ArgumentException>(() => Email.From("invalid-email"));
        Assert.Throws<ArgumentException>(() => Email.From("@example.com"));
        Assert.Throws<ArgumentException>(() => Email.From("test@"));
    }
    
    [Fact]
    public void PhoneNumber_ShouldValidatePhoneFormat()
    {
        var validPhone = "+1234567890";
        var phoneWithFormatting = "+1 (234) 567-890";
        var phone1 = PhoneNumber.From(validPhone);
        var phone2 = PhoneNumber.From(phoneWithFormatting);
        
        Assert.Equal("+1234567890", phone1.Value);
        Assert.Equal("+1234567890", phone2.Value);
        
        string implicitString = phone1;
        Assert.Equal("+1234567890", implicitString);
        
        Assert.Throws<ArgumentException>(() => PhoneNumber.From(""));
        Assert.Throws<ArgumentException>(() => PhoneNumber.From("1234567890"));
        Assert.Throws<ArgumentException>(() => PhoneNumber.From("+"));
        Assert.Throws<ArgumentException>(() => PhoneNumber.From("invalid"));
    }
    
    [Fact]
    public void Slug_ShouldGenerateValidSlugs()
    {
        var title = "Hello World Test";
        var slug1 = Slug.From(title);
        var slug2 = Slug.FromString("hello-world-test");
        
        Assert.Equal("hello-world-test", slug1.Value);
        Assert.Equal(slug1, slug2);
        
        string implicitString = slug1;
        Assert.Equal("hello-world-test", implicitString);
        
        var slugWithUnderscore = Slug.From("test_value");
        Assert.Equal("test-value", slugWithUnderscore.Value);
        
        Assert.Throws<ArgumentException>(() => Slug.From(""));
        Assert.Throws<ArgumentException>(() => Slug.FromString("Invalid Slug"));
        Assert.Throws<ArgumentException>(() => Slug.FromString("invalid_slug"));
        Assert.Throws<ArgumentException>(() => Slug.FromString("INVALID"));
    }
}