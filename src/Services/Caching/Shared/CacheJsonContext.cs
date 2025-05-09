using System.Text.Json.Serialization;

namespace AzureMcp.Services.Caching.Shared;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTime?))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(DistributedCacheEntry))]
public partial class CacheJsonContext : JsonSerializerContext
{
}

public sealed record DistributedCacheEntry(byte[] Value, DateTimeOffset? AbsoluteExpiration, TimeSpan? SlidingExpiration);
