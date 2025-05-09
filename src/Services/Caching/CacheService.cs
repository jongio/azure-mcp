// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMcp.Services.Caching.Providers;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AzureMcp.Services.Caching;

public class CacheService : ICacheService
{
    private readonly ICacheProvider[] _providers;

    public CacheService(IEnumerable<ICacheProvider> providers)
    {
        _providers = providers as ICacheProvider[] ?? providers.ToArray();
    }

    public CacheService(IMemoryCache memoryCache)
        : this(new[] { new MemoryCacheProvider(memoryCache) })
    {
    }

    public async ValueTask<T?> GetAsync<T>(string key, TimeSpan? expiration = null)
    {
        foreach (var provider in _providers)
        {
            var result = await provider.GetAsync<T>(key);
            if (result != null)
                return result;
        }
        return default;
    }

    public async ValueTask SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        if (data == null)
            return;

        foreach (var provider in _providers)
        {
            await provider.SetAsync(key, data, expiration);
        }
    }

    public async ValueTask DeleteAsync(string key)
    {
        foreach (var provider in _providers)
        {
            await provider.DeleteAsync(key);
        }
    }
}
