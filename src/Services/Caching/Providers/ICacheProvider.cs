using System;
using System.Threading.Tasks;

namespace AzureMcp.Services.Caching.Providers;

public interface ICacheProvider
{
    ValueTask<T?> GetAsync<T>(string key);
    ValueTask SetAsync<T>(string key, T data, TimeSpan? expiration = null);
    ValueTask DeleteAsync(string key);
}
