// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using AzureMcp.Services.Caching.Shared;
using Xunit;

namespace AzureMcp.Tests.Services.Caching;

public class CacheJsonContextTests
{
    [Fact]
    public void SerializeDateTime_Succeeds()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var json = JsonSerializer.Serialize(date, CacheJsonContext.Default.DateTime);
        var result = JsonSerializer.Deserialize(json, CacheJsonContext.Default.DateTime);

        // Assert
        Assert.Equal(date, result);
    }

    [Fact]
    public void SerializeDictionary_Succeeds()
    {
        // Arrange
        var dict = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        var json = JsonSerializer.Serialize(dict, CacheJsonContext.Default.DictionaryStringString);
        var result = JsonSerializer.Deserialize(json, CacheJsonContext.Default.DictionaryStringString);

        // Assert
        Assert.Equal(dict, result);
    }

    [Fact]
    public void SerializeDistributedCacheEntry_Succeeds()
    {
        // Arrange
        var entry = new DistributedCacheEntry(
            new byte[] { 1, 2, 3 },
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(5));

        // Act
        var json = JsonSerializer.Serialize(entry, CacheJsonContext.Default.DistributedCacheEntry);
        var result = JsonSerializer.Deserialize(json, CacheJsonContext.Default.DistributedCacheEntry);

        // Assert
        Assert.Equal(entry.Value, result?.Value);
        Assert.Equal(entry.AbsoluteExpiration, result?.AbsoluteExpiration);
        Assert.Equal(entry.SlidingExpiration, result?.SlidingExpiration);
    }
}
