// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using AzureMcp.Services.Caching.Providers;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AzureMcp.Tests.Services.Caching;

public class MemoryCacheProviderTests
{
    private readonly MemoryCache _memoryCache;
    private readonly MemoryCacheProvider _sut;
    private const string TestKey = "test-key";
    private const string TestValue = "test-value";
    private static readonly TimeSpan TestExpiration = TimeSpan.FromMinutes(5);
    private static readonly TestObject TestObjectValue = new() { Id = 42, Name = "Test Object" };

    public MemoryCacheProviderTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _sut = new MemoryCacheProvider(_memoryCache);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsDefault()
    {
        // Act
        var result = await _sut.GetAsync<string>(TestKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsValue()
    {
        // Arrange
        _memoryCache.Set(TestKey, TestValue);

        // Act
        var result = await _sut.GetAsync<string>(TestKey);

        // Assert
        Assert.Equal(TestValue, result);
    }

    [Fact]
    public async Task SetAsync_WithValue_StoresInCache()
    {
        // Act
        await _sut.SetAsync(TestKey, TestValue);

        // Assert
        Assert.True(_memoryCache.TryGetValue(TestKey, out string? value));
        Assert.Equal(TestValue, value);
    }

    [Fact]
    public async Task SetAsync_WithNullValue_DoesNotStoreInCache()
    {
        // Arrange
        string? nullValue = null;

        // Act
        await _sut.SetAsync(TestKey, nullValue);

        // Assert
        Assert.False(_memoryCache.TryGetValue(TestKey, out _));
    }

    [Fact]
    public async Task SetAsync_WithExpiration_SetsExpirationTime()
    {
        // Act
        await _sut.SetAsync(TestKey, TestValue, TestExpiration);

        // Note: We can't test the actual expiration directly without introducing timing issues,
        // but we can verify something is in the cache
        Assert.True(_memoryCache.TryGetValue(TestKey, out string? value));
        Assert.Equal(TestValue, value);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingKey()
    {
        // Arrange
        _memoryCache.Set(TestKey, TestValue);
        Assert.True(_memoryCache.TryGetValue(TestKey, out _), "Setup failed - value not in cache");

        // Act
        await _sut.DeleteAsync(TestKey);

        // Assert
        Assert.False(_memoryCache.TryGetValue(TestKey, out _));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentKey_DoesNotThrow()
    {
        // Act & Assert - should not throw
        await _sut.DeleteAsync("non-existent-key");
    }

    [Fact]
    public async Task GetAsync_WithComplexType_ReturnsValue()
    {
        // Arrange
        _memoryCache.Set(TestKey, TestObjectValue);

        // Act
        var result = await _sut.GetAsync<TestObject>(TestKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestObjectValue.Id, result.Id);
        Assert.Equal(TestObjectValue.Name, result.Name);
    }

    [Fact]
    public async Task SetAsync_WithComplexType_StoresInCache()
    {
        // Act
        await _sut.SetAsync(TestKey, TestObjectValue);

        // Assert
        Assert.True(_memoryCache.TryGetValue(TestKey, out TestObject? value));
        Assert.NotNull(value);
        Assert.Equal(TestObjectValue.Id, value.Id);
        Assert.Equal(TestObjectValue.Name, value.Name);
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ReturnsDefault()
    {
        // Act
        var result = await _sut.GetAsync<string>(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.GetAsync<string>(null!));
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.SetAsync(null!, TestValue));
    }

    [Fact]
    public async Task DeleteAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.DeleteAsync(null!));
    }

    [Fact]
    public async Task SetAsync_OverwritesExistingValue()
    {
        // Arrange
        await _sut.SetAsync(TestKey, "original-value");
        Assert.True(_memoryCache.TryGetValue(TestKey, out string? originalValue));
        Assert.Equal("original-value", originalValue);

        // Act
        await _sut.SetAsync(TestKey, TestValue);

        // Assert
        Assert.True(_memoryCache.TryGetValue(TestKey, out string? newValue));
        Assert.Equal(TestValue, newValue);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
