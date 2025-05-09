using AzureMcp.Services.Caching.Providers;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AzureMcp.Services.Caching.Extensions;

public static class CachingServiceCollectionExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ICacheProvider, MemoryCacheProvider>();
        services.AddScoped<ICacheProvider, FileCacheProvider>();
        services.AddScoped<ICacheService, CacheService>();
        return services;
    }
}
