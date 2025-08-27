using SharedPlatform.Features.DomainPrimitives.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.Entities;

public sealed class ServiceAggregate : BaseAggregateRoot
{
    public Service Service { get; private set; }
    public List<ServiceMetadata> AdditionalMetadata { get; private set; }
    public List<string> Tags { get; private set; }
    public Dictionary<string, string> LocalizedContent { get; private set; }

    public override object Id => Service?.ServiceId.Value ?? Guid.NewGuid();

    private ServiceAggregate()
    {
        Service = Service.Create(ServiceTitle.From("Default Service"), ServiceDescription.From("Default Description"));
        AdditionalMetadata = new List<ServiceMetadata>();
        Tags = new List<string>();
        LocalizedContent = new Dictionary<string, string>();
    }

    private ServiceAggregate(Service service)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));
        AdditionalMetadata = new List<ServiceMetadata>();
        Tags = new List<string>();
        LocalizedContent = new Dictionary<string, string>();
    }

    public static ServiceAggregate Create(Service service)
    {
        return new ServiceAggregate(service);
    }

    public void AddMetadata(ServiceMetadata metadata)
    {
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));
        
        AdditionalMetadata.Add(metadata);
    }

    public void RemoveMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));
        
        AdditionalMetadata.RemoveAll(m => m.Key == key);
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty", nameof(tag));
        
        var normalizedTag = tag.Trim().ToLowerInvariant();
        if (!Tags.Contains(normalizedTag))
        {
            Tags.Add(normalizedTag);
        }
    }

    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty", nameof(tag));
        
        Tags.Remove(tag.Trim().ToLowerInvariant());
    }

    public void SetLocalizedContent(string languageCode, string content)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new ArgumentException("Language code cannot be null or empty", nameof(languageCode));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));
        
        LocalizedContent[languageCode.ToLowerInvariant()] = content;
    }

    public void RemoveLocalizedContent(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new ArgumentException("Language code cannot be null or empty", nameof(languageCode));
        
        LocalizedContent.Remove(languageCode.ToLowerInvariant());
    }

    public string? GetLocalizedContent(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return null;
        
        return LocalizedContent.TryGetValue(languageCode.ToLowerInvariant(), out var content) ? content : null;
    }

    public ServiceMetadata? GetMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;
        
        return AdditionalMetadata.FirstOrDefault(m => m.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }
}