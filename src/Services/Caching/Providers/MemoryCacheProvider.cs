using System;
using System.Threading.Tasks;
using AzureMcp.Services.Caching.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace AzureMcp.Services.Caching.Providers;

public class MemoryCacheProvider(IMemoryCache memoryCache) : ICacheProvider
{
    public ValueTask<T?> GetAsync<T>(string key)
        => memoryCache.TryGetValue(key, out T? value) ? new ValueTask<T?>(value) : default;

    public ValueTask SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        if (data == null)
            return default;

        memoryCache.Set(key, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });

        return default;
    }

    public ValueTask DeleteAsync(string key)
    {
        memoryCache.Remove(key);
        return default;
    }
}
