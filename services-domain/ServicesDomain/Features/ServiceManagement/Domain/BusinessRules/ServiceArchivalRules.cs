using ServicesDomain.Features.ServiceManagement.Domain.Entities;

namespace ServicesDomain.Features.ServiceManagement.Domain.BusinessRules;

public static class ServiceArchivalRules
{
    public static bool CanBeArchived(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return !service.IsDeleted && service.PublishingStatus.IsDraft;
    }

    public static bool IsArchived(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return service.PublishingStatus.IsArchived;
    }

    public static string[] ValidateArchival(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        if (service.IsDeleted)
            return new[] { "Cannot archive deleted service" };
            
        if (service.PublishingStatus.IsArchived)
            return new[] { "Service is already archived" };
            
        return Array.Empty<string>();
    }

    public static bool CanBeUnarchived(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return service.PublishingStatus.IsArchived && !service.IsDeleted;
    }
}