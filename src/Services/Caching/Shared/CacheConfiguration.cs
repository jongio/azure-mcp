using System.Text.Json;

namespace AzureMcp.Services.Caching.Shared;

public static class CacheConfiguration
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        TypeInfoResolver = CacheJsonContext.Default
    };
}
