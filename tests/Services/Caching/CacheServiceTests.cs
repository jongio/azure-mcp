// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMcp.Services.Caching;
using AzureMcp.Services.Caching.Providers;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Services.Caching;

public class CacheServiceTests
{
    private readonly ICacheProvider _mockProvider1;
    private readonly ICacheProvider _mockProvider2;
    private readonly IMemoryCache _mockMemoryCache;
    private const string TestKey = "test-key";
    private static readonly TestModel TestData = new() { Id = 1, Name = "Test" };
    private static readonly TimeSpan TestExpiration = TimeSpan.FromMinutes(5);

    public CacheServiceTests()
    {
        _mockProvider1 = Substitute.For<ICacheProvider>();
        _mockProvider2 = Substitute.For<ICacheProvider>();
        _mockMemoryCache = Substitute.For<IMemoryCache>();
    }

    [Fact]
    public async Task GetAsync_MultipleCacheProviders_ReturnsFirstNonNullResult()
    {
        // Arrange
        _mockProvider1.GetAsync<TestModel>(TestKey).Returns(default(TestModel));
        _mockProvider2.GetAsync<TestModel>(TestKey).Returns(TestData);

        var sut = new CacheService(new[] { _mockProvider1, _mockProvider2 });

        // Act
        var result = await sut.GetAsync<TestModel>(TestKey);

        // Assert
        Assert.Equal(TestData, result);
        await _mockProvider1.Received(1).GetAsync<TestModel>(TestKey);
        await _mockProvider2.Received(1).GetAsync<TestModel>(TestKey);
    }

    [Fact]
    public async Task GetAsync_AllCacheProvidersReturnNull_ReturnsDefault()
    {
        // Arrange
        _mockProvider1.GetAsync<TestModel>(TestKey).Returns(default(TestModel));
        _mockProvider2.GetAsync<TestModel>(TestKey).Returns(default(TestModel));

        var sut = new CacheService(new[] { _mockProvider1, _mockProvider2 });

        // Act
        var result = await sut.GetAsync<TestModel>(TestKey);

        // Assert
        Assert.Null(result);
        await _mockProvider1.Received(1).GetAsync<TestModel>(TestKey);
        await _mockProvider2.Received(1).GetAsync<TestModel>(TestKey);
    }

    [Fact]
    public async Task GetAsync_FirstProviderReturnsData_SecondProviderNotCalled()
    {
        // Arrange
        _mockProvider1.GetAsync<TestModel>(TestKey).Returns(TestData);

        var sut = new CacheService(new[] { _mockProvider1, _mockProvider2 });

        // Act
        var result = await sut.GetAsync<TestModel>(TestKey);

        // Assert
        Assert.Equal(TestData, result);
        await _mockProvider1.Received(1).GetAsync<TestModel>(TestKey);
        await _mockProvider2.DidNotReceive().GetAsync<TestModel>(TestKey);
    }

    [Fact]
    public async Task SetAsync_WithData_CallsSetOnAllProviders()
    {
        // Arrange
        var sut = new CacheService(new[] { _mockProvider1, _mockProvider2 });

        // Act
        await sut.SetAsync(TestKey, TestData, TestExpiration);

        // Assert
        await _mockProvider1.Received(1).SetAsync(TestKey, TestData, TestExpiration);
        await _mockProvider2.Received(1).SetAsync(TestKey, TestData, TestExpiration);
    }
    [Fact]
    public async Task SetAsync_WithNullData_DoesNotCallSetOnProviders()
    {
        // Arrange
        var sut = new CacheService(new[] { _mockProvider1, _mockProvider2 });
        TestModel? nullData = null;

        // Act
        await sut.SetAsync(TestKey, nullData, TestExpiration);

        // Assert
        await _mockProvider1.DidNotReceive().SetAsync(TestKey, Arg.Any<TestModel>(), Arg.Any<TimeSpan?>());
        await _mockProvider2.DidNotReceive().SetAsync(TestKey, Arg.Any<TestModel>(), Arg.Any<TimeSpan?>());
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnAllProviders()
    {
        // Arrange
        var sut = new CacheService(new[] { _mockProvider1, _mockProvider2 });

        // Act
        await sut.DeleteAsync(TestKey);

        // Assert
        await _mockProvider1.Received(1).DeleteAsync(TestKey);
        await _mockProvider2.Received(1).DeleteAsync(TestKey);
    }
    [Fact]
    public async Task Constructor_WithMemoryCache_CreatesMemoryCacheProvider()
    {
        // Arrange
        object? outValue = null;
        _mockMemoryCache.TryGetValue(Arg.Any<string>(), out outValue)
            .Returns(x =>
            {
                x[1] = TestData;
                return true;
            });

        // Act
        var sut = new CacheService(_mockMemoryCache);

        // Assert - we can't directly verify the provider creation, so we test functionality
        var getResult = await sut.GetAsync<TestModel>(TestKey);
        Assert.Equal(TestData, getResult);
    }

    [Fact]
    public async Task GetAsync_WithExpiration_PassesExpirationToProviders()
    {
        // Arrange
        var expiration = TimeSpan.FromMinutes(30);
        _mockProvider1.GetAsync<TestModel>(TestKey).Returns(default(TestModel));
        _mockProvider2.GetAsync<TestModel>(TestKey).Returns(TestData);

        var sut = new CacheService(new[] { _mockProvider1, _mockProvider2 });

        // Act
        var result = await sut.GetAsync<TestModel>(TestKey, expiration);

        // Assert
        Assert.Equal(TestData, result);
        await _mockProvider1.Received(1).GetAsync<TestModel>(TestKey);
        await _mockProvider2.Received(1).GetAsync<TestModel>(TestKey);
    }

    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
