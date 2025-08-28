using ServicesDomain.Features.ServiceManagement.Domain.Entities;

namespace ServicesDomain.Features.ServiceManagement.Domain.BusinessRules;

public static class ServiceBusinessRules
{
    public static bool IsValidStatus(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return service.PublishingStatus != null;
    }

    public static string[] ValidateService(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation  
        var errors = new List<string>();
        
        if (service.Title == null)
            errors.Add("Title is required");
            
        if (service.Description == null)
            errors.Add("Short description is required");
            
        if (service.DeliveryMode == null)
            errors.Add("Delivery mode is required");
            
        return errors.ToArray();
    }

    public static bool CanBeDeleted(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return !service.IsDeleted;
    }

    public static bool IsActive(Service service)
    {
        // TODO: Implement in TDD phase - stub for compilation
        return service.PublishingStatus.IsPublished && !service.IsDeleted;
    }
}