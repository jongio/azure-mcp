using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AzureMcp.Services.Caching.Shared;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Services.Caching.Providers;

public class FileCacheProvider : ICacheProvider
{
    private readonly string _cacheFolder;

    public FileCacheProvider()
    {
        _cacheFolder = Path.Combine(Path.GetTempPath(), "AzureMcpCache");
        Directory.CreateDirectory(_cacheFolder);
    }

    public ValueTask<T?> GetAsync<T>(string key)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath))
            return new ValueTask<T?>(default(T));

        var fileContent = File.ReadAllText(filePath);
        var entry = JsonSerializer.Deserialize(fileContent, CacheJsonContext.Default.DistributedCacheEntry);

        if (entry == null)
            return new ValueTask<T?>(default(T));

        if (entry.AbsoluteExpiration.HasValue && entry.AbsoluteExpiration.Value <= DateTimeOffset.UtcNow)
        {
            File.Delete(filePath);
            return new ValueTask<T?>(default(T));
        }

        if (entry.Value == null)
            return new ValueTask<T?>(default(T));

        var json = Encoding.UTF8.GetString(entry.Value);
        var typeInfo = CacheJsonContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
        if (typeInfo == null)
            throw new InvalidOperationException($"Unable to get JsonTypeInfo for type {typeof(T)}.");
        return new ValueTask<T?>(JsonSerializer.Deserialize(json, typeInfo));
    }

    public ValueTask SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        if (data == null)
            return default;

        var typeInfo = CacheJsonContext.Default.GetTypeInfo(typeof(T));
        if (typeInfo == null)
            throw new InvalidOperationException($"Unable to get JsonTypeInfo for type {typeof(T)}.");

        var json = JsonSerializer.Serialize(data, typeInfo);
        var bytes = Encoding.UTF8.GetBytes(json);

        var entry = new DistributedCacheEntry(
            bytes,
            expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : null,
            null);

        var filePath = GetFilePath(key);
        var serialized = JsonSerializer.Serialize(entry, CacheJsonContext.Default.DistributedCacheEntry);
        File.WriteAllText(filePath, serialized);

        return default;
    }

    public ValueTask DeleteAsync(string key)
    {
        var filePath = GetFilePath(key);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return default;
    }

    private string GetFilePath(string key) =>
        Path.Combine(_cacheFolder, Convert.ToBase64String(Encoding.UTF8.GetBytes(key)));
}
