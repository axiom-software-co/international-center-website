namespace ServicesDomain.Shared.Extensions;

public static class WebApplicationExtensions 
{ 
    public static WebApplication UseServicesDomainPipeline(this WebApplication app)
    {
        // Basic pipeline configuration - minimal implementation for compilation
        app.UseRouting();
        return app;
    }
}
