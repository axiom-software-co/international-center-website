using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedPlatform.Features.ContentManagement.Abstractions;
using SharedPlatform.Features.ContentManagement.Configuration;
using SharedPlatform.Features.ContentManagement.Services;

namespace SharedPlatform.Features.ContentManagement.Extensions;

public static class ContentManagementExtensions
{
    public static IServiceCollection AddContentManagement(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register configuration options
        services.Configure<ContentStorageOptions>(configuration.GetSection(ContentStorageOptions.SectionName));
        services.Configure<CdnOptions>(configuration.GetSection(CdnOptions.SectionName));
        services.Configure<ContentLifecycleOptions>(configuration.GetSection(ContentLifecycleOptions.SectionName));
        
        // Register core services
        services.AddSingleton<IContentHashService, ContentHashService>();
        services.AddSingleton<IContentUrlGenerator, CdnUrlGenerator>();
        services.AddScoped<IContentStorageService, ContentStorageService>();
        services.AddScoped<IContentAuditService, ContentAuditService>();
        services.AddScoped<IContentRetrievalService, ContentRetrievalService>();
        services.AddScoped<IContentLifecycleService, ContentLifecycleService>();
        
        return services;
    }

    public static IServiceCollection AddContentManagement(this IServiceCollection services, 
        Action<ContentStorageOptions> configureStorage,
        Action<CdnOptions>? configureCdn = null,
        Action<ContentLifecycleOptions>? configureLifecycle = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureStorage);

        // Register configuration options with delegates
        services.Configure(configureStorage);
        
        if (configureCdn != null)
            services.Configure(configureCdn);
        
        if (configureLifecycle != null)
            services.Configure(configureLifecycle);
        
        // Register core services
        services.AddSingleton<IContentHashService, ContentHashService>();
        services.AddSingleton<IContentUrlGenerator, CdnUrlGenerator>();
        services.AddScoped<IContentStorageService, ContentStorageService>();
        services.AddScoped<IContentAuditService, ContentAuditService>();
        services.AddScoped<IContentRetrievalService, ContentRetrievalService>();
        services.AddScoped<IContentLifecycleService, ContentLifecycleService>();
        
        return services;
    }

    public static IServiceCollection AddBlobStorageContent(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register specific Azure Blob Storage configuration
        services.Configure<ContentStorageOptions>(configuration.GetSection(ContentStorageOptions.SectionName));
        
        // Register storage-specific services
        services.AddScoped<IContentStorageService, ContentStorageService>();
        services.AddScoped<IContentRetrievalService, ContentRetrievalService>();
        
        return services;
    }

    public static IServiceCollection AddContentAuditing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<IContentAuditService, ContentAuditService>();
        
        return services;
    }

    public static IServiceCollection AddContentLifecycleManagement(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ContentLifecycleOptions>(configuration.GetSection(ContentLifecycleOptions.SectionName));
        services.AddScoped<IContentLifecycleService, ContentLifecycleService>();
        
        return services;
    }
}