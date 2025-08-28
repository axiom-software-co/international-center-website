using ServicesDomain.Features.ServiceManagement.Domain.Entities;

namespace ServicesDomain.Features.ServiceManagement.Domain.BusinessRules;

public static class ServicePublishingRules
{
    public static bool CanBePublished(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return !service.IsDeleted && !service.PublishingStatus.IsArchived;
    }

    public static bool IsPublished(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return service.PublishingStatus.IsPublished;
    }

    public static string[] ValidatePublishing(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        if (service.IsDeleted)
            return new[] { "Cannot publish deleted service" };
            
        if (service.PublishingStatus.IsArchived)
            return new[] { "Cannot publish archived service" };
            
        return Array.Empty<string>();
    }

    public static bool CanBeUnpublished(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return service.PublishingStatus.IsPublished;
    }
}